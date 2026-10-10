using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Game.Systems.SteamNetwork
{
    public class SteamNetworkDispatcher : IDisposable
    {
        private delegate void P2PMessageHandlerDelegate(CSteamID senderId, IntPtr dataPtr, int dataSize);
        private delegate void LobbyMessageHandlerDelegate(CSteamID senderId, BinaryReader reader);

        // Хранилища обработчиков по MessageId
        private readonly Dictionary<ushort, P2PMessageHandlerDelegate> _p2pHandlers = new();
        private readonly Dictionary<ushort, LobbyMessageHandlerDelegate> _lobbyHandlers = new();

        // Буферы для фазы лобби
        private readonly MemoryStream _lobbySendStream = new(2048);
        private readonly BinaryWriter _lobbyWriter;

        // Callbacks Steam
        private Callback<LobbyChatMsg_t> _lobbyChatMsgCallback;
        private CSteamID _currentLobbyId = CSteamID.Nil;

        public SteamNetworkDispatcher()
        {
            _lobbyWriter = new BinaryWriter(_lobbySendStream);
            _lobbyChatMsgCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessageReceived);
        }

        public void SetLobby(CSteamID lobbyId)
        {
            _currentLobbyId = lobbyId;
        }

        #region Handler Registration

        /// <summary>
        /// Регистрация обработчика для P2P-сокетов (структуры фиксированного размера через Marshal)
        /// </summary>
        public void RegisterP2PHandler<T>(Action<CSteamID, T> handler) where T : struct, INetworkMessage
        {
            T dummy = default;
            ushort msgId = dummy.MessageId;

            _p2pHandlers[msgId] = (senderId, dataPtr, dataSize) =>
            {
                if (dataSize < Marshal.SizeOf<T>())
                {
                    Debug.LogError($"[Dispatcher] P2P message size mismatch for ID {msgId}. Expected: {Marshal.SizeOf<T>()}, Received: {dataSize}");
                    return;
                }

                T msg = Marshal.PtrToStructure<T>(dataPtr);
                handler?.Invoke(senderId, msg);
            };
        }

        /// <summary>
        /// Регистрация обработчика для сообщений Лобби (динамические данные через BinaryReader)
        /// </summary>
        public void RegisterLobbyHandler<T>(Action<CSteamID, T> handler, Func<BinaryReader, T> deserializer) where T : INetworkMessage, new()
        {
            T dummy = new T();
            ushort msgId = dummy.MessageId;

            _lobbyHandlers[msgId] = (senderId, reader) =>
            {
                T msg = deserializer(reader);
                handler?.Invoke(senderId, msg);
            };
        }

        public void UnregisterP2PHandler<T>() where T : struct, INetworkMessage
        {
            T dummy = default;
            _p2pHandlers.Remove(dummy.MessageId);
        }

        public void UnregisterLobbyHandler<T>() where T : INetworkMessage, new()
        {
            T dummy = new T();
            _lobbyHandlers.Remove(dummy.MessageId);
        }

        #endregion

        #region Gameplay (P2P Sockets via Marshal)

        /// <summary>
        /// Отправка C#-структуры через SteamNetworkingSockets с помощью Marshal (Zero-GC)
        /// </summary>
        public bool SendP2P<T>(HSteamNetConnection connection, T message, int sendFlags = Constants.k_nSteamNetworkingSend_Reliable) where T : struct, INetworkMessage
        {
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(message, ptr, false);

                EResult result = SteamNetworkingSockets.SendMessageToConnection(
                    connection,
                    ptr,
                    (uint)size,
                    sendFlags,
                    out _
                );

                return result == EResult.k_EResultOK;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Опрос входящих P2P-пакетов по соединению (вызывается в Update)
        /// </summary>
        public void PollP2PMessages(HSteamNetConnection connection)
        {
            IntPtr[] messageBuffer = new IntPtr[16];
            int count = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messageBuffer, messageBuffer.Length);

            for (int i = 0; i < count; i++)
            {
                SteamNetworkingMessage_t netMsg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messageBuffer[i]);

                // Первые 2 байта любая наша P2P-структура содержит MessageId
                if (netMsg.m_cbSize >= sizeof(ushort))
                {
                    ushort msgId = (ushort)Marshal.ReadInt16(netMsg.m_pData);

                    if (_p2pHandlers.TryGetValue(msgId, out var handler))
                    {
                        CSteamID senderId = netMsg.m_identityPeer.GetSteamID();
                        handler.Invoke(senderId, netMsg.m_pData, netMsg.m_cbSize);
                    }
                    else
                    {
                        Debug.LogWarning($"[Dispatcher] Unhandled P2P MessageID: {msgId}");
                    }
                }

                // Освобождаем нативную память сообщения Steam
                SteamNetworkingMessage_t.Release(messageBuffer[i]);
            }
        }

        #endregion

        #region Lobby Phase (SteamMatchmaking)

        /// <summary>
        /// Отправка сообщения участникам Лобби через SteamMatchmaking.SendLobbyChatMsg
        /// </summary>
        public bool SendLobbyMessage<T>(T message, Action<BinaryWriter> serializer) where T : INetworkMessage
        {
            if (!_currentLobbyId.IsValid())
            {
                Debug.LogWarning("[Dispatcher] Cannot send lobby message: Invalid Lobby ID.");
                return false;
            }

            _lobbySendStream.SetLength(0);

            // 1. Записываем 2 байта MessageId
            _lobbyWriter.Write(message.MessageId);

            // 2. Сериализуем данные
            serializer(_lobbyWriter);
            _lobbyWriter.Flush();

            byte[] data = _lobbySendStream.ToArray();

            return SteamMatchmaking.SendLobbyChatMsg(_currentLobbyId, data, data.Length);
        }

        private void OnLobbyChatMessageReceived(LobbyChatMsg_t callback)
        {
            CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            if (_currentLobbyId != CSteamID.Nil && lobbyID != _currentLobbyId)
                return;

            byte[] buffer = new byte[4096];

            int bytesRead = SteamMatchmaking.GetLobbyChatEntry(
                lobbyID,
                (int)callback.m_iChatID,
                out CSteamID senderID,
                buffer,
                buffer.Length,
                out EChatEntryType chatEntryType
            );

            if (bytesRead >= sizeof(ushort) && chatEntryType == EChatEntryType.k_EChatEntryTypeChatMsg)
            {
                using (MemoryStream recvStream = new MemoryStream(buffer, 0, bytesRead))
                using (BinaryReader reader = new BinaryReader(recvStream))
                {
                    ushort msgId = reader.ReadUInt16();

                    if (_lobbyHandlers.TryGetValue(msgId, out var handler))
                    {
                        handler.Invoke(senderID, reader);
                    }
                }
            }
        }

        #endregion

        public void Dispose()
        {
            _lobbyChatMsgCallback?.Dispose();
            _lobbyChatMsgCallback = null;
            _p2pHandlers.Clear();
            _lobbyHandlers.Clear();
        }
    }
}