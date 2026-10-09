using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Game.Systems.SteamNetwork
{
    /// <summary>
    /// Диспетчер сетевых сообщений на базе SteamNetworkingSockets
    /// </summary>
    public class SteamNetworkDispatcher
    {
        private delegate void MessageHandlerDelegate(HSteamNetConnection conn, BinaryReader reader);

        // Словарь для хранения обработчиков по MessageId
        private readonly Dictionary<ushort, MessageHandlerDelegate> _handlers = new();

        // Кешированные буферы для работы без лишнего GC
        private readonly MemoryStream _sendStream = new(4096);
        private readonly BinaryWriter _writer;

        public SteamNetworkDispatcher()
        {
            _writer = new BinaryWriter(_sendStream);
        }

        #region Registration

        /// <summary>
        /// Регистрация обработчика входящих сообщений типа T
        /// </summary>
        public void RegisterHandler<T>(Action<HSteamNetConnection, T> handler) where T : INetworkMessage, new()
        {
            T dummy = new T();
            ushort msgId = dummy.MessageId;

            _handlers[msgId] = (conn, reader) =>
            {
                T msg = new T();
                msg.Deserialize(reader);
                handler?.Invoke(conn, msg);
            };
        }

        /// <summary>
        /// Отмена регистрации обработчика сообщений типа T
        /// </summary>
        public void UnregisterHandler<T>() where T : INetworkMessage, new()
        {
            T dummy = new T();
            _handlers.Remove(dummy.MessageId);
        }

        #endregion

        #region Sending

        /// <summary>
        /// Отправка сообщения конкретному соединению
        /// </summary>
        public bool Send<T>(HSteamNetConnection connection, T message, int sendFlags = Constants.k_nSteamNetworkingSend_Reliable) where T : INetworkMessage
        {
            _sendStream.SetLength(0);

            // 1. Записываем ID сообщения (2 байта)
            _writer.Write(message.MessageId);

            // 2. Сериализуем полезную нагрузку
            message.Serialize(_writer);
            _writer.Flush();

            byte[] buffer = _sendStream.GetBuffer();
            int length = (int)_sendStream.Length;

            // Передача байтов через SteamNetworkingSockets
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                EResult result = SteamNetworkingSockets.SendMessageToConnection(connection, ptr, (uint)length, sendFlags, out _);
                return result == EResult.k_EResultOK;
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Массовая отправка сообщения списку подключений
        /// </summary>
        public void Broadcast<T>(IReadOnlyList<HSteamNetConnection> connections, T message, int sendFlags = Constants.k_nSteamNetworkingSend_Reliable) where T : INetworkMessage
        {
            for (int i = 0; i < connections.Count; i++)
            {
                Send(connections[i], message, sendFlags);
            }
        }

        #endregion

        #region Polling / Receiving

        /// <summary>
        /// Чтение входящих сообщений через группу опроса (Server / Host)
        /// </summary>
        public void PollIncomingMessages(HSteamNetPollGroup pollGroup, int maxMessages = 32)
        {
            IntPtr[] msgPointers = new IntPtr[maxMessages];

            // В Steamworks.NET сообщения с группы серверов читаются через ReceiveMessagesOnPollGroup
            int numMsgs = SteamNetworkingSockets.ReceiveMessagesOnPollGroup(pollGroup, msgPointers, maxMessages);

            if (numMsgs > 0)
            {
                ProcessMessages(msgPointers, numMsgs);
            }
        }

        /// <summary>
        /// Чтение входящих сообщений с точечного соединения (Client)
        /// </summary>
        public void PollIncomingMessages(HSteamNetConnection connection, int maxMessages = 32)
        {
            IntPtr[] msgPointers = new IntPtr[maxMessages];

            // Чтение для одиночного клиента
            int numMsgs = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, msgPointers, maxMessages);

            if (numMsgs > 0)
            {
                ProcessMessages(msgPointers, numMsgs);
            }
        }

        private void ProcessMessages(IntPtr[] msgPointers, int count)
        {
            for (int i = 0; i < count; i++)
            {
                // Получаем структуру сообщения по указателю
                SteamNetworkingMessage_t netMsg = SteamNetworkingMessage_t.FromIntPtr(msgPointers[i]);

                byte[] data = new byte[netMsg.m_cbSize];
                Marshal.Copy(netMsg.m_pData, data, 0, netMsg.m_cbSize);

                using (MemoryStream recvStream = new MemoryStream(data))
                using (BinaryReader reader = new BinaryReader(recvStream))
                {
                    if (recvStream.Length >= sizeof(ushort))
                    {
                        ushort msgId = reader.ReadUInt16();

                        if (_handlers.TryGetValue(msgId, out var handler))
                        {
                            handler.Invoke(netMsg.m_conn, reader);
                        }
                        else
                        {
                            Debug.LogWarning($"[SteamNetwork] Неизвестный MessageID: {msgId}");
                        }
                    }
                }

                // Освобождаем нативную память Steam
                SteamNetworkingMessage_t.Release(msgPointers[i]);
            }
        }

        #endregion
    }
}