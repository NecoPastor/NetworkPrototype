using Steamworks;
using System;
using UnityEngine;

namespace Game.Systems
{
    public class SteamNetworkManager : MonoBehaviour
    {
        [Header("Steam Settings")]
        public uint appId = 480;
        [SerializeField] private bool onlyInBuildInitialized;

        public bool IsInitialized { get; private set; }
        public CSteamID TargetHostSteamID { get; set; } = CSteamID.Nil;
        public CSteamID CurrentLobbyID { get; private set; } = CSteamID.Nil;

        // События состояния подключения
        public event Action OnSteamConnected;
        public event Action OnSteamDisconnected;
        public event Action<CSteamID> OnLobbyStateChanged;

        private Callback<LobbyCreated_t> _lobbyCreatedCallback;
        private Callback<LobbyEnter_t> _lobbyEnterCallback;

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

                // Регистрируем отслеживание лобби
                _lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
                _lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

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

            // Обработка внутренних событий Steam
            SteamAPI.RunCallbacks();

            if (!SteamUser.BLoggedOn())
            {
                HandleDisconnection("[Steamworks.NET] Lost connection to Steam servers.");
            }
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                OnLobbyStateChanged?.Invoke(CurrentLobbyID);
            }
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            OnLobbyStateChanged?.Invoke(CurrentLobbyID);
        }

        public void LeaveLobby()
        {
            if (CurrentLobbyID != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(CurrentLobbyID);
                CurrentLobbyID = CSteamID.Nil;
                OnLobbyStateChanged?.Invoke(CurrentLobbyID);
            }
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

                    _lobbyCreatedCallback?.Dispose();
                    _lobbyEnterCallback?.Dispose();

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
            IsInitialized = false;
            CurrentLobbyID = CSteamID.Nil;
            OnSteamDisconnected?.Invoke();
        }

        private void Log(string message)
        {
            Debug.Log(message);
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }
    }
}