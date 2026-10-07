using Steamworks;
using System;
using TMPro;
using UnityEngine;

public class LobbyNetworkManager : MonoBehaviour
{
    public static LobbyNetworkManager Instance { get; private set; }

    public event Action<CSteamID, bool> OnLobbyCreatedEvent;
    public event Action<bool> OnLobbyJoinedEvent;

    public CSteamID CurrentLobbyID { get; private set; }

    private Callback<LobbyCreated_t> m_LobbyCreated;
    private Callback<GameLobbyJoinRequested_t> m_LobbyJoinRequested;
    private Callback<LobbyEnter_t> m_LobbyEntered;
    private Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;

    [SerializeField] private TMP_Text textDebug;

    private void Awake()
    {
        Application.runInBackground = true;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!SteamAPI.IsSteamRunning())
        {
            string message = "[LobbyNetworkManager] Steam is not running!";
            Debug.LogError(message);
            SetDebugText(message);
            return;
        }

        m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        m_LobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        m_LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);

        CheckCommandLineInvite();
    }

    private void Update()
    {
        if (SteamAPI.IsSteamRunning())
        {
            SteamAPI.RunCallbacks();
        }
    }

    public void CreateLobby()
    {
        string message = "[LobbyNetworkManager] Sending CreateLobby request...";
        Debug.Log(message);
        SetDebugText(message);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 999);
    }

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

        SteamMatchmaking.SetLobbyData(CurrentLobbyID, "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(CurrentLobbyID, "name", $"{SteamFriends.GetPersonaName()}'s Game");

        SteamFriends.SetRichPresence("connect", $"+connect_lobby {CurrentLobbyID}");

        string successMessage = $"[LobbyNetworkManager] Lobby Created! Waiting for player... (ID: {CurrentLobbyID})";
        Debug.Log(successMessage);
        SetDebugText(successMessage);
        OnLobbyCreatedEvent?.Invoke(CurrentLobbyID, true);
    }

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
        string successMessage = $"[LobbyNetworkManager] Successfully in lobby! Member count: {SteamMatchmaking.GetNumLobbyMembers(CurrentLobbyID)}";
        Debug.Log(successMessage);
        SetDebugText(successMessage);

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
        if (CurrentLobbyID.IsValid() && SteamUtils.IsOverlayEnabled())
        {
            string message = "[LobbyNetworkManager] Opening Steam Invite Dialog...";
            Debug.Log(message);
            SetDebugText(message);
            SteamFriends.ActivateGameOverlayInviteDialog(CurrentLobbyID);
        }
        else
        {
            string warningMessage = "[LobbyNetworkManager] Steam Overlay is disabled or Lobby ID is invalid!";
            Debug.LogWarning(warningMessage);
            SetDebugText(warningMessage);
        }
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