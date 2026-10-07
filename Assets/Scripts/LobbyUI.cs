using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button inviteButton;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        //if (createLobbyButton != null)
        //{
        //    createLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
        //}
        if (SteamAPI.IsSteamRunning())
        {
            OnCreateLobbyButtonClicked();
        }

        if (inviteButton != null)
        {
            inviteButton.onClick.AddListener(OnInviteButtonClicked);
            inviteButton.interactable = false;
        }

        if (LobbyNetworkManager.Instance != null)
        {
            LobbyNetworkManager.Instance.OnLobbyCreatedEvent += OnLobbyCreated;
            LobbyNetworkManager.Instance.OnLobbyJoinedEvent += OnLobbyJoined;
        }

        UpdateStatus("Ready to connect");
    }

    private void OnDestroy()
    {
        if (LobbyNetworkManager.Instance != null)
        {
            LobbyNetworkManager.Instance.OnLobbyCreatedEvent -= OnLobbyCreated;
            LobbyNetworkManager.Instance.OnLobbyJoinedEvent -= OnLobbyJoined;
        }
    }

    private void OnCreateLobbyButtonClicked()
    {
        UpdateStatus("Creating lobby...");
        LobbyNetworkManager.Instance.CreateLobby();
    }

    private void OnInviteButtonClicked()
    {
        LobbyNetworkManager.Instance.OpenInviteOverlay();
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