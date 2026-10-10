using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Systems.SteamNetwork
{
    public class SteamNetworkManager : MonoBehaviour
    {
        [Header("Steam Settings")]
        public uint appId = 480;
        [SerializeField] private bool onlyInBuildInitialized;

        public bool IsInitialized { get; private set; }
        public CSteamID TargetHostSteamID { get; set; } = CSteamID.Nil;
        public CSteamID CurrentLobbyID { get; private set; } = CSteamID.Nil;

        private SteamNetworkDispatcher _dispatcher;

        // P2P Сокеты и соединения Steam
        private HSteamListenSocket _listenSocket = HSteamListenSocket.Invalid;
        private HSteamNetConnection _clientConnection = HSteamNetConnection.Invalid;

        // Список активных подключений клиентов (на стороне Хоста)
        private readonly List<HSteamNetConnection> _activeConnections = new();
        public IReadOnlyList<HSteamNetConnection> ActiveConnections => _activeConnections;

        // События состояния подключения и лобби
        public event Action OnSteamConnected;
        public event Action OnSteamDisconnected;
        public event Action<CSteamID> OnLobbyStateChanged;

        // Коллбэки Steam API
        private Callback<LobbyCreated_t> _lobbyCreatedCallback;
        private Callback<LobbyEnter_t> _lobbyEnterCallback;
        private Callback<SteamNetConnectionStatusChangedCallback_t> _connectionStatusCallback;

        private void Start()
        {
            InitializeSteam();
        }

        public void InitializeSteam()
        {
            if (onlyInBuildInitialized && Application.isEditor)
            {
                Debug.LogWarning("[Steamworks.NET] Initialization skipped: Running inside Unity Editor.");
                return;
            }

            if (IsInitialized) return;

            if (Application.isEditor)
            {
                try
                {
                    System.IO.File.WriteAllText("steam_appid.txt", appId.ToString());
                }
                catch (Exception e)
                {
                    LogError($"[Steamworks.NET] Could not write steam_appid.txt: {e.Message}");
                }
            }

            if (!Packsize.Test())
            {
                LogError("[Steamworks.NET] Packsize Test failed. Wrong DLL or platform version.");
                return;
            }

            if (!DllCheck.Test())
            {
                LogError("[Steamworks.NET] DllCheck Test failed. Missing steam_api64.dll or steam_api.dll.");
                return;
            }

            try
            {
                IsInitialized = SteamAPI.Init();

                if (!IsInitialized)
                {
                    LogWarning("[Steamworks.NET] SteamAPI_Init() failed. Is Steam running?");
                    return;
                }

                // Создаем приватный Диспетчер сообщений
                _dispatcher = new SteamNetworkDispatcher();

                // Регистрируем коллбэки лобби и P2P-подключений
                _lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
                _lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
                _connectionStatusCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);

                string personName = SteamFriends.GetPersonaName();
                CSteamID steamId = SteamUser.GetSteamID();
                Log($"[Steamworks.NET] Connected! Player: {personName} ({steamId})");

                OnSteamConnected?.Invoke();
            }
            catch (Exception e)
            {
                LogError($"[Steamworks.NET] Exception during init: {e.Message}");
            }
        }

        private void Update()
        {
            if (!IsInitialized) return;

            // 1. Обработка внутренних коллбэков Steam
            SteamAPI.RunCallbacks();

            if (!SteamUser.BLoggedOn())
            {
                HandleDisconnection("[Steamworks.NET] Lost connection to Steam servers.");
                return;
            }

            // 2. Опрашиваем входящие P2P-пакеты, если мы Клиент
            if (_clientConnection != HSteamNetConnection.Invalid)
            {
                _dispatcher?.PollP2PMessages(_clientConnection);
            }

            // 3. Опрашиваем входящие P2P-пакеты от всех клиентов, если мы Хост
            if (_activeConnections.Count > 0)
            {
                for (int i = 0; i < _activeConnections.Count; i++)
                {
                    _dispatcher?.PollP2PMessages(_activeConnections[i]);
                }
            }
        }

        #region Dispatcher API Wrapper

        /// <summary>
        /// Регистрация обработчика P2P-сообщений (структуры с Marshal)
        /// </summary>
        public void RegisterP2PHandler<T>(Action<CSteamID, T> handler) where T : struct, INetworkMessage
        {
            _dispatcher?.RegisterP2PHandler(handler);
        }

        /// <summary>
        /// Отмена регистрации обработчика P2P-сообщений
        /// </summary>
        public void UnregisterP2PHandler<T>() where T : struct, INetworkMessage
        {
            _dispatcher?.UnregisterP2PHandler<T>();
        }

        /// <summary>
        /// Регистрация обработчика сообщений Лобби (BinaryReader)
        /// </summary>
        public void RegisterLobbyHandler<T>(Action<CSteamID, T> handler, Func<BinaryReader, T> deserializer) where T : INetworkMessage, new()
        {
            _dispatcher?.RegisterLobbyHandler(handler, deserializer);
        }

        public void SetLobby(CSteamID currentLobby)
        {
            _dispatcher?.SetLobby(currentLobby);
        }

        /// <summary>
        /// Отмена регистрации обработчика сообщений Лобби
        /// </summary>
        public void UnregisterLobbyHandler<T>() where T : INetworkMessage, new()
        {
            _dispatcher?.UnregisterLobbyHandler<T>();
        }

        /// <summary>
        /// Отправка сообщения участникам Лобби
        /// </summary>
        public bool SendLobbyMessage<T>(T message, Action<BinaryWriter> serializer) where T : INetworkMessage
        {
            if (_dispatcher == null) return false;
            return _dispatcher.SendLobbyMessage(message, serializer);
        }

        #endregion

        #region Host / Server Operations

        /// <summary>
        /// Открыть P2P-сокет прослушивания (Вызывается Хостом)
        /// </summary>
        public bool StartHost()
        {
            if (!IsInitialized) return false;

            _listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
            bool success = _listenSocket != HSteamListenSocket.Invalid;

            if (success)
            {
                Log("[SteamNetworkManager] P2P Host Listen Socket opened.");
            }
            else
            {
                LogError("[SteamNetworkManager] Failed to open P2P Host Listen Socket!");
            }

            return success;
        }

        #endregion

        #region Client Operations

        /// <summary>
        /// Подключиться к Хосту по его SteamID (Вызывается Клиентом)
        /// </summary>
        public bool ConnectToHost(CSteamID hostSteamID)
        {
            if (!IsInitialized) return false;

            SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
            identity.SetSteamID(hostSteamID);

            _clientConnection = SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, null);
            bool success = _clientConnection != HSteamNetConnection.Invalid;

            if (success)
            {
                Log($"[SteamNetworkManager] Connecting P2P to host: {hostSteamID}...");
            }
            else
            {
                LogError($"[SteamNetworkManager] Failed to initiate P2P connection to host: {hostSteamID}");
            }

            return success;
        }

        #endregion

        #region Convenience Send Methods

        /// <summary>
        /// Отправить P2P-сообщение Хосту (для Клиента)
        /// </summary>
        public bool SendToServer<T>(T message, int sendFlags = Constants.k_nSteamNetworkingSend_Reliable) where T : struct, INetworkMessage
        {
            if (_clientConnection == HSteamNetConnection.Invalid || _dispatcher == null) return false;
            return _dispatcher.SendP2P(_clientConnection, message, sendFlags);
        }

        /// <summary>
        /// Рассылка P2P-сообщения всем подключенным Клиентам (для Хоста)
        /// </summary>
        public void BroadcastToClients<T>(T message, int sendFlags = Constants.k_nSteamNetworkingSend_Reliable) where T : struct, INetworkMessage
        {
            if (_dispatcher == null) return;

            for (int i = 0; i < _activeConnections.Count; i++)
            {
                _dispatcher.SendP2P(_activeConnections[i], message, sendFlags);
            }
        }

        #endregion

        #region Lobby Handlers & State

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                UpdateLobbyID(new CSteamID(callback.m_ulSteamIDLobby));
            }
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            UpdateLobbyID(new CSteamID(callback.m_ulSteamIDLobby));
        }

        public void LeaveLobby()
        {
            if (CurrentLobbyID != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(CurrentLobbyID);
                UpdateLobbyID(CSteamID.Nil);
            }
        }

        private void UpdateLobbyID(CSteamID lobbyID)
        {
            CurrentLobbyID = lobbyID;
            _dispatcher?.SetLobby(lobbyID);
            OnLobbyStateChanged?.Invoke(CurrentLobbyID);
        }

        #endregion

        #region Steam Connection Callbacks

        private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t param)
        {
            // Новое входящее подключение клиенту на стороне Хоста
            if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting &&
                param.m_info.m_hListenSocket == _listenSocket)
            {
                EResult res = SteamNetworkingSockets.AcceptConnection(param.m_hConn);
                if (res == EResult.k_EResultOK)
                {
                    _activeConnections.Add(param.m_hConn);
                    Log($"[SteamNetworkManager] Host accepted P2P connection from: {param.m_info.m_identityRemote.GetSteamID()}");
                }
            }
            // Отключение или обрыв связи
            else if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer ||
                     param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
            {
                if (_activeConnections.Contains(param.m_hConn))
                {
                    _activeConnections.Remove(param.m_hConn);
                }

                if (_clientConnection == param.m_hConn)
                {
                    _clientConnection = HSteamNetConnection.Invalid;
                }

                SteamNetworkingSockets.CloseConnection(param.m_hConn, 0, "Connection Closed", false);
                LogWarning($"[SteamNetworkManager] Connection closed: {param.m_info.m_szEndDebug}");
            }
        }

        #endregion

        #region Cleanup & Shutdown

        public void CloseSockets()
        {
            if (_listenSocket != HSteamListenSocket.Invalid)
            {
                SteamNetworkingSockets.CloseListenSocket(_listenSocket);
                _listenSocket = HSteamListenSocket.Invalid;
            }

            if (_clientConnection != HSteamNetConnection.Invalid)
            {
                SteamNetworkingSockets.CloseConnection(_clientConnection, 0, "Closing app", false);
                _clientConnection = HSteamNetConnection.Invalid;
            }

            for (int i = 0; i < _activeConnections.Count; i++)
            {
                SteamNetworkingSockets.CloseConnection(_activeConnections[i], 0, "Closing host", false);
            }
            _activeConnections.Clear();
        }

        private void OnDisable()
        {
            _connectionStatusCallback?.Dispose();
            _connectionStatusCallback = null;
        }

        private void OnApplicationQuit()
        {
            ShutdownSteam();
        }

        public void ShutdownSteam()
        {
            if (IsInitialized)
            {
                try
                {
                    LeaveLobby();
                    CloseSockets();

                    _lobbyCreatedCallback?.Dispose();
                    _lobbyEnterCallback?.Dispose();
                    _connectionStatusCallback?.Dispose();

                    _dispatcher?.Dispose();
                    _dispatcher = null;

                    SteamAPI.Shutdown();
                    Log("[Steamworks.NET] SteamAPI Shutdown completed.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Steamworks.NET] Shutdown exception: {e.Message}");
                }
                finally
                {
                    IsInitialized = false;
                    OnSteamDisconnected?.Invoke();
                }
            }
        }

        private void HandleDisconnection(string reason)
        {
            LogError(reason);
            CloseSockets();
            IsInitialized = false;
            CurrentLobbyID = CSteamID.Nil;
            OnSteamDisconnected?.Invoke();
        }

        private void Log(string message) => Debug.Log(message);
        private void LogWarning(string message) => Debug.LogWarning(message);
        private void LogError(string message) => Debug.LogError(message);



        #endregion
    }
}