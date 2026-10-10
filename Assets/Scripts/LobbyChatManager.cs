using Game.Systems.Service;
using Game.Systems.SteamNetwork;
using Steamworks;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyChatManager : MonoBehaviour
{
    public event Action<CSteamID, string> OnChatMessageReceived;

    [SerializeField] private LobbyNetworkManager lobby;

    [Header("UI References")]
    [SerializeField] private TMP_Text textChatDisplay;
    [SerializeField] private TMP_InputField inputFieldMessage;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect chatScrollRect;

    private SteamNetworkManager networkService;

    private void Start()
    {
        if (ServiceLocator.TryGetService(out SteamNetworkManager service))
        {
            networkService = service;
        }

        if (networkService == null)
        {
            Debug.LogWarning("[LobbyChatManager] SteamNetworkService is not available. Chat disabled.");
            return;
        }

        RegisterHandlers();
        BindUI();
    }

    private void RegisterHandlers()
    {
        networkService.RegisterLobbyHandler<ChatMessage>(OnChatMessageReceivedHandler, ChatMessage.Deserialize);
    }

    private void UnregisterHandlers()
    {
        if (networkService != null)
        {
            networkService.UnregisterLobbyHandler<ChatMessage>();
        }
    }

    private void BindUI()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(SendChatMessage);

        if (inputFieldMessage != null)
            inputFieldMessage.onSubmit.AddListener(OnInputSubmit);
    }

    public void SendChatMessage()
    {
        if (inputFieldMessage == null || string.IsNullOrWhiteSpace(inputFieldMessage.text))
            return;

        if (lobby == null)
            return;

        CSteamID currentLobby = lobby.CurrentLobbyID;
        if (!currentLobby.IsValid())
            return;

        // Синхронизируем ID лобби с диспетчером перед отправкой
        networkService.SetLobby(currentLobby);

        string messageText = inputFieldMessage.text.Trim();
        ChatMessage msg = new ChatMessage(messageText);

        bool success = networkService.SendLobbyMessage(msg, writer => msg.Serialize(writer));
        if (success)
        {
            inputFieldMessage.text = string.Empty;
        }
        else
        {
            Debug.LogError("[LobbyChatManager] Failed to send lobby chat message via Dispatcher.");
        }
    }

    private void OnChatMessageReceivedHandler(CSteamID senderID, ChatMessage msg)
    {
        string message = msg.Content;
        string senderName = SteamFriends.GetFriendPersonaName(senderID);
        CSteamID mySteamID = SteamUser.GetSteamID();

        string formattedMessage = senderID == mySteamID
            ? $"<i>{senderName}</i>: {message}"
            : $"<color=yellow><i>{senderName}</i>: {message}</color>";

        AppendToChatDisplay(formattedMessage);
        OnChatMessageReceived?.Invoke(senderID, message);
    }

    private void AppendToChatDisplay(string message)
    {
        if (textChatDisplay == null) return;

        if (string.IsNullOrEmpty(textChatDisplay.text))
        {
            textChatDisplay.text = message;
        }
        else
        {
            textChatDisplay.text += $"\n{message}";
        }

        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        if (chatScrollRect != null)
        {
            StartCoroutine(ScrollToBottomCoroutine());
        }
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    private void OnInputSubmit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendChatMessage();
            StartCoroutine(ReactivateInputNextFrame());
        }
    }

    private IEnumerator ReactivateInputNextFrame()
    {
        yield return null;
        if (inputFieldMessage != null)
        {
            inputFieldMessage.ActivateInputField();
            inputFieldMessage.Select();
        }
    }

    private void OnDisable()
    {
        UnregisterHandlers();
    }

    private void OnDestroy()
    {
        if (sendButton != null)
            sendButton.onClick.RemoveListener(SendChatMessage);

        if (inputFieldMessage != null)
            inputFieldMessage.onSubmit.RemoveListener(OnInputSubmit);

        UnregisterHandlers();
    }
}