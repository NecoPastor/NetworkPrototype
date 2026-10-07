using Steamworks;
using System;
using TMPro;
using UnityEngine;

namespace Game.Systems
{
    public class NetworkManager : MonoBehaviour
    {
        [Header("Steam Settings")]
        public uint appId = 480;
        [SerializeField] private bool onlyInBuildInitialized;

        [Header("Auto Reconnect Settings")]
        [SerializeField] private bool autoReconnect = true;
        [SerializeField] private float reconnectInterval = 5f;

        [Header("Debug")]
        [SerializeField] private TMP_Text textDebug;

        public bool IsInitialized { get; private set; }
        public CSteamID TargetHostSteamID { get; set; } = CSteamID.Nil;

        // События состояния подключения
        public event Action OnSteamConnected;
        public event Action OnSteamDisconnected;

        private float reconnectTimer;

        public void InitializeSteam()
        {
            if (onlyInBuildInitialized && Application.isEditor)
            {
                Debug.LogWarning("[Steamworks.NET] Initialization skipped: Running inside Unity Editor.");
                return;
            }

            if (IsInitialized) return;

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
                System.IO.File.WriteAllText("steam_appid.txt", appId.ToString());

                IsInitialized = SteamAPI.Init();

                if (!IsInitialized)
                {
                    LogWarning("[Steamworks.NET] SteamAPI_Init() failed. Is Steam running?");
                    return;
                }

                string personName = SteamFriends.GetPersonaName();
                CSteamID steamId = SteamUser.GetSteamID();
                Log($"[Steamworks.NET] Connected! Player: {personName} ({steamId})");

                reconnectTimer = 0f;
                OnSteamConnected?.Invoke();
            }
            catch (Exception e)
            {
                LogError($"[Steamworks.NET] Exception during init: {e.Message}");
            }
        }

        private void Update()
        {
            if (!IsInitialized)
            {
                // Если авто-переподключение включено, проверяем таймер
                if (autoReconnect)
                {
                    reconnectTimer += Time.deltaTime;
                    if (reconnectTimer >= reconnectInterval)
                    {
                        reconnectTimer = 0f;
                        Log("[Steamworks.NET] Attempting to reconnect to Steam...");
                        InitializeSteam();
                    }
                }
                return;
            }

            // Обработка внутренних событий Steam
            SteamAPI.RunCallbacks();

            // Проверка активности соединения с серверами Steam
            if (!SteamUser.BLoggedOn())
            {
                HandleDisconnection("[Steamworks.NET] Lost connection to Steam servers.");
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
                    SteamAPI.RunCallbacks();
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
            reconnectTimer = 0f; // Сбрасываем таймер, чтобы следующая попытка пошла с нуля
            OnSteamDisconnected?.Invoke();
        }

        private void Log(string message)
        {
            Debug.Log(message);
            if (textDebug != null)
            {
                textDebug.text = message;
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning(message);
            if (textDebug != null)
            {
                textDebug.text = message;
            }
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
            if (textDebug != null)
            {
                textDebug.text = message;
            }
        }
    }
}