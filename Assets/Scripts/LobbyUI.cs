using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private LobbyNetworkManager lobby;
    [Header("UI References")]
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button inviteButton;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        if (createLobbyButton != null)
        {
            CreateLobby();
        }

        if (inviteButton != null)
        {
            inviteButton.onClick.AddListener(OnInviteButtonClicked);
            inviteButton.interactable = false;
        }

        lobby.OnLobbyCreatedEvent += OnLobbyCreated;
        lobby.OnLobbyJoinedEvent += OnLobbyJoined;

        UpdateStatus("Ready to connect");
    }

    private void OnDestroy()
    {
        if (lobby != null)
        {
            lobby.OnLobbyCreatedEvent -= OnLobbyCreated;
            lobby.OnLobbyJoinedEvent -= OnLobbyJoined;
        }
    }

    private void CreateLobby()
    {
        UpdateStatus("Creating lobby...");
        lobby.CreateLobby();
    }

    private void OnInviteButtonClicked()
    {
        lobby.OpenInviteOverlay();
    }

    private void OnLobbyCreated(CSteamID lobbyId, bool success)
    {
        if (success)
        {
            UpdateStatus($"Lobby Created! ID: {lobbyId}");

            if (inviteButton != null)
            {
                inviteButton.interactable = true;
            }
        }
        else
        {
            UpdateStatus("Failed to create lobby!");
        }
    }

    private void OnLobbyJoined(bool success)
    {
        if (success)
        {
            UpdateStatus("Joined Lobby!");
            if (inviteButton != null)
            {
                inviteButton.interactable = true;
            }
        }
        else
        {
            UpdateStatus("Failed to join lobby!");
        }
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log($"[LobbyUI] {message}");
    }
}