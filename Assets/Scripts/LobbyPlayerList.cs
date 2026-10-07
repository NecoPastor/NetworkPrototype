using Steamworks;
using TMPro;
using UnityEngine;

public class LobbyPlayerList : MonoBehaviour
{
    [SerializeField] private TMP_Text playerListText;

    private Callback<LobbyEnter_t> lobbyEnterCallback;
    private Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback;
    private CSteamID currentLobbyID;

    private void OnEnable()
    {
        if (SteamAPI.IsSteamRunning())
        {
            lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        }
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (callback.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            UpdatePlayerList();
        }
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        if (new CSteamID(callback.m_ulSteamIDLobby) == currentLobbyID)
        {
            UpdatePlayerList();
        }
    }

    public void UpdatePlayerList()
    {
        if (!SteamAPI.IsSteamRunning() || !currentLobbyID.IsValid() || playerListText == null)
        {
            return;
        }

        CSteamID hostID = SteamMatchmaking.GetLobbyOwner(currentLobbyID);

        int memberCount = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);
        string resultText = string.Empty;

        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyID, i);
            string memberName = SteamFriends.GetFriendPersonaName(memberID);

            if (memberID == hostID)
            {
                resultText += $"{memberName} <color=yellow>(Host)</color>\n";
            }
            else
            {
                resultText += $"{memberName}\n";
            }
        }

        playerListText.text = resultText;
    }
}