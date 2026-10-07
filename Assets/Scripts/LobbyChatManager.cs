using Game.Systems;
using Game.Systems.Service;
using Steamworks;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyChatManager : MonoBehaviour
{
    public event Action<CSteamID, string> OnChatMessageReceived;

    private Callback<LobbyChatMsg_t> m_LobbyChatMsg;

    [SerializeField] private LobbyNetworkManager lobby;

    [Header("UI References")]
    [SerializeField] private TMP_Text textChatDisplay;
    [SerializeField] private TMP_InputField inputFieldMessage;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect chatScrollRect;

    private NetworkManager networkManager;

    private void Start()
    {
        if (ServiceLocator.TryGetService(out NetworkManager provider))
        {
            networkManager = provider;
        }

        if (networkManager == null || !networkManager.IsInitialized)
        {
            Debug.LogWarning("[LobbyChatManager] NetworkManager provider is not initialized. Chat disabled.");
            return;
        }

        RegisterCallbacks();
        BindUI();
    }

    private void RegisterCallbacks()
    {
        m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
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

        string messageText = inputFieldMessage.text.Trim();

        // Steam C++ API ожидает null-terminated UTF-8 строку (\0)
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(messageText + "\0");

        bool success = SteamMatchmaking.SendLobbyChatMsg(currentLobby, messageBytes, messageBytes.Length);
        if (success)
        {
            inputFieldMessage.text = string.Empty;
        }
        else
        {
            Debug.LogError("[LobbyChatManager] Failed to send lobby chat message.");
        }
    }

    private void OnLobbyChatMessage(LobbyChatMsg_t callback)
    {
        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        // Проверяем, что сообщение пришло именно из нашего текущего лобби
        if (lobby != null && lobbyID != lobby.CurrentLobbyID)
            return;

        byte[] data = new byte[4096];

        int bytesRead = SteamMatchmaking.GetLobbyChatEntry(
            lobbyID,
            (int)callback.m_iChatID,
            out CSteamID senderID,
            data,
            data.Length,
            out EChatEntryType chatEntryType
        );

        if (bytesRead > 0 && chatEntryType == EChatEntryType.k_EChatEntryTypeChatMsg)
        {
            // Декодируем и убираем концевой \0
            string message = System.Text.Encoding.UTF8.GetString(data, 0, bytesRead).TrimEnd('\0');
            string senderName = SteamFriends.GetFriendPersonaName(senderID);
            CSteamID mySteamID = SteamUser.GetSteamID();

            string formattedMessage = senderID == mySteamID
                ? $"<i>{senderName}</i>: {message}"
                : $"<color=yellow><i>{senderName}</i>: {message}</color>";

            AppendToChatDisplay(formattedMessage);
            OnChatMessageReceived?.Invoke(senderID, message);
        }
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
        UnregisterCallbacks();
    }

    private void UnregisterCallbacks()
    {
        m_LobbyChatMsg?.Dispose();
        m_LobbyChatMsg = null;
    }

    private void OnDestroy()
    {
        if (sendButton != null)
            sendButton.onClick.RemoveListener(SendChatMessage);

        if (inputFieldMessage != null)
            inputFieldMessage.onSubmit.RemoveListener(OnInputSubmit);

    }
}