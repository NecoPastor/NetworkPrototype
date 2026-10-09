using FishNet;
using Game.Systems;
using Game.Systems.Service;
using Steamworks;
using System;
using TMPro;
using UnityEngine;

public class LobbyNetworkManager : MonoBehaviour
{
    public event Action<CSteamID, bool> OnLobbyCreatedEvent;
    public event Action<bool> OnLobbyJoinedEvent;
    public CSteamID CurrentLobbyID { get; private set; }
    public CSteamID HostSteamID { get; private set; }
    private Callback<LobbyCreated_t> m_LobbyCreated;
    private Callback<GameLobbyJoinRequested_t> m_LobbyJoinRequested;
    private Callback<LobbyEnter_t> m_LobbyEntered;
    private Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;

    [SerializeField] private TMP_Text textDebug;

    private NetworkManager networkManager;

    private void Start()
    {
        if (ServiceLocator.TryGetService(out NetworkManager provider))
            networkManager = provider;

        if (networkManager == null)
        {
            string message = "[LobbyNetworkManager] NetworkManager service is null!";
            Debug.LogError(message);
            SetDebugText(message);
            return;
        }

        if (!networkManager.IsInitialized)
        {
            string message = "[LobbyNetworkManager] NetworkManager service is not initialized!";
            Debug.LogError(message);
            SetDebugText(message);
            return;
        }

        RegisterCallbacks();
        CheckCommandLineInvite();
    }

    private void RegisterCallbacks()
    {
        m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        m_LobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        m_LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
    }

    private void UnregisterCallbacks()
    {
        m_LobbyCreated?.Dispose();
        m_LobbyCreated = null;

        m_LobbyJoinRequested?.Dispose();
        m_LobbyJoinRequested = null;

        m_LobbyEntered?.Dispose();
        m_LobbyEntered = null;

        m_LobbyChatUpdate?.Dispose();
        m_LobbyChatUpdate = null;
    }

    public void CreateLobby()
    {
        string message = "[LobbyNetworkManager] Sending CreateLobby request...";
        Debug.Log(message);
        SetDebugText(message);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 999);
    }

    //DenEdit
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            string errorMessage = $"[LobbyNetworkManager] Failed to create lobby: {callback.m_eResult}";
            Debug.LogError(errorMessage);
            SetDebugText(errorMessage);
            OnLobbyCreatedEvent?.Invoke(CSteamID.Nil, false);
            return;
        }

        CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        HostSteamID = SteamUser.GetSteamID();

        if (networkManager != null)
        {
            networkManager.TargetHostSteamID = HostSteamID;
        }

        // Данные для подключения клиентов по поиску или инвайтам
        SteamMatchmaking.SetLobbyData(CurrentLobbyID, "HostAddress", HostSteamID.ToString());
        SteamMatchmaking.SetLobbyData(CurrentLobbyID, "name", $"{SteamFriends.GetPersonaName()}'s Game");

        // Интеграция с быстрыми инвайтами через Steam Overlay
        SteamFriends.SetRichPresence("connect", $"+connect_lobby {CurrentLobbyID}");

        string successMessage = $"[LobbyNetworkManager] Lobby Created! Waiting for player... (ID: {CurrentLobbyID})";
        Debug.Log(successMessage);
        SetDebugText(successMessage);

        // Безопасный запуск FishNet
        var netManager = InstanceFinder.NetworkManager;
        if (netManager == null)
        {
            Debug.LogError("[LobbyNetworkManager] FishNet NetworkManager not found!");
            OnLobbyCreatedEvent?.Invoke(CurrentLobbyID, false);
            return;
        }

        if (!netManager.IsServerStarted && !netManager.IsClientStarted)
        {
            // Передаем SteamID хоста в транспорт FishySteamworks
            if (netManager.TransportManager.Transport is FishySteamworks.FishySteamworks fishySteamworks)
            {
                fishySteamworks.SetClientAddress(HostSteamID.ToString());
            }
            else
            {
                Debug.LogWarning("[LobbyNetworkManager] Transport is not FishySteamworks! Address not explicitly set.");
            }

            netManager.ServerManager.StartConnection();
            netManager.ClientManager.StartConnection();
            Debug.Log("[LobbyNetworkManager] FishNet Host started successfully!");
        }

        OnLobbyCreatedEvent?.Invoke(CurrentLobbyID, true);
    }
    //private void OnLobbyCreated(LobbyCreated_t callback)
    //{
    //    if (callback.m_eResult != EResult.k_EResultOK)
    //    {
    //        string errorMessage = $"[LobbyNetworkManager] Failed to create lobby: {callback.m_eResult}";
    //        Debug.LogError(errorMessage);
    //        SetDebugText(errorMessage);
    //        OnLobbyCreatedEvent?.Invoke(CSteamID.Nil, false);
    //        return;
    //    }

    //    CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
    //    HostSteamID = SteamUser.GetSteamID();

    //    // Сохраняем ID хоста в сервис NetworkManager
    //    if (networkManager != null)
    //    {
    //        networkManager.TargetHostSteamID = HostSteamID;
    //    }

    //    SteamMatchmaking.SetLobbyData(CurrentLobbyID, "HostAddress", HostSteamID.ToString());
    //    SteamMatchmaking.SetLobbyData(CurrentLobbyID, "name", $"{SteamFriends.GetPersonaName()}'s Game");

    //    SteamFriends.SetRichPresence("connect", $"+connect_lobby {CurrentLobbyID}");

    //    string successMessage = $"[LobbyNetworkManager] Lobby Created! Waiting for player... (ID: {CurrentLobbyID})";
    //    Debug.Log(successMessage);
    //    SetDebugText(successMessage);


    //    if (!InstanceFinder.IsServer && !InstanceFinder.IsClient)
    //    {
    //        InstanceFinder.NetworkManager.ServerManager.StartConnection();
    //        InstanceFinder.NetworkManager.ClientManager.StartConnection();
    //        Debug.Log("[LobbyNetworkManager] FishNet Host started successfully!");
    //    }

    //    OnLobbyCreatedEvent?.Invoke(CurrentLobbyID, true);
    //}

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        string message = $"[LobbyNetworkManager] Invite clicked! Joining lobby: {callback.m_steamIDLobby}...";
        Debug.Log(message);
        SetDebugText(message);
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            string errorMessage = $"[LobbyNetworkManager] Failed to enter lobby. Response code: {callback.m_EChatRoomEnterResponse}";
            Debug.LogError(errorMessage);
            SetDebugText(errorMessage);
            OnLobbyJoinedEvent?.Invoke(false);
            return;
        }

        CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        // Извлекаем SteamID хоста
        string hostAddressStr = SteamMatchmaking.GetLobbyData(CurrentLobbyID, "HostAddress");
        if (ulong.TryParse(hostAddressStr, out ulong hostSteamIDUlong))
        {
            HostSteamID = new CSteamID(hostSteamIDUlong);
            if (networkManager != null)
            {
                networkManager.TargetHostSteamID = HostSteamID;
            }
        }

        string successMessage = $"[LobbyNetworkManager] Successfully in lobby! Member count: {SteamMatchmaking.GetNumLobbyMembers(CurrentLobbyID)}";
        Debug.Log(successMessage);
        SetDebugText(successMessage);


        // --- FishNet Integration ---
        if (HostSteamID == SteamUser.GetSteamID())
        {
            // Host already started network connection in OnLobbyCreated, do nothing here to avoid resetting state.
        }
        else
        {
            // If we are a client, pass host address to transport and start client connection
            if (InstanceFinder.IsClient)
            {
                var transport = InstanceFinder.NetworkManager.GetComponentInChildren<FishySteamworks.FishySteamworks>();
                if (transport != null)
                {
                    transport.SetClientAddress(hostAddressStr);
                }

                InstanceFinder.NetworkManager.ClientManager.StartConnection();
                Debug.Log($"[LobbyNetworkManager] FishNet Client started, connecting to host: {hostAddressStr}...");
            }
        }
        // ---------------------------


        OnLobbyJoinedEvent?.Invoke(true);
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        if (callback.m_ulSteamIDLobby != CurrentLobbyID.m_SteamID) return;

        EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

        if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
        {
            CSteamID newPlayerID = new CSteamID(callback.m_ulSteamIDUserChanged);
            string playerName = SteamFriends.GetFriendPersonaName(newPlayerID);

            string message = $"[LobbyNetworkManager] Player connected: {playerName} ({newPlayerID})";
            Debug.Log(message);
            SetDebugText(message);
        }
    }

    public void OpenInviteOverlay()
    {
        // 1. Проверка валидности лобби
        if (!CurrentLobbyID.IsValid())
        {
            string errorMsg = $"[LobbyNetworkManager] Failed to open invite! Invalid Lobby ID: {CurrentLobbyID}";
            Debug.LogWarning(errorMsg);
            SetDebugText(errorMsg);
            return;
        }

        // 2. Проверка доступности оверлея Steam
        if (!SteamUtils.IsOverlayEnabled())
        {
            string warningMsg = "[LobbyNetworkManager] Failed to open invite! Steam Overlay is disabled or unavailable.";
            Debug.LogWarning(warningMsg);
            SetDebugText(warningMsg);
            return;
        }

        // Успешный сценарий
        string successMsg = "[LobbyNetworkManager] Opening Steam Invite Dialog...";
        Debug.Log(successMsg);
        SetDebugText(successMsg);

        SteamFriends.ActivateGameOverlayInviteDialog(CurrentLobbyID);
    }

    private void CheckCommandLineInvite()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "+connect_lobby" && i + 1 < args.Length)
            {
                if (ulong.TryParse(args[i + 1], out ulong lobbyID))
                {
                    CSteamID lobbySteamID = new CSteamID(lobbyID);
                    string message = $"[LobbyNetworkManager] Launching via invite to lobby: {lobbySteamID}...";
                    Debug.Log(message);
                    SetDebugText(message);
                    SteamMatchmaking.JoinLobby(lobbySteamID);
                }
            }
        }
    }

    private void SetDebugText(string message)
    {
        if (textDebug != null)
        {
            textDebug.text = message;
        }
    }
}