using Steamworks;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyChatManager : MonoBehaviour
{
    public static LobbyChatManager Instance { get; private set; }

    public event Action<CSteamID, string> OnChatMessageReceived;

    private Callback<LobbyChatMsg_t> m_LobbyChatMsg;

    [SerializeField] private TMP_Text textChatDisplay;
    [SerializeField] private TMP_InputField inputFieldMessage;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect chatScrollRect;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (SteamAPI.IsSteamRunning())
        {
            m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);

            sendButton.onClick.AddListener(sendButton_Click);
            inputFieldMessage.onSubmit.AddListener(OnInputSubmit);
        }
    }

    public void sendButton_Click()
    {
        if (inputFieldMessage == null || string.IsNullOrWhiteSpace(inputFieldMessage.text)) return;

        CSteamID currentLobby = LobbyNetworkManager.Instance.CurrentLobbyID;
        if (!currentLobby.IsValid()) return;

        string messageText = inputFieldMessage.text;
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(messageText);

        bool success = SteamMatchmaking.SendLobbyChatMsg(currentLobby, messageBytes, messageBytes.Length + 1);
        if (success)
        {
            inputFieldMessage.text = string.Empty;
        }
    }

    private void OnLobbyChatMessage(LobbyChatMsg_t callback)
    {
        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID senderID = new CSteamID(callback.m_ulSteamIDUser);

        byte[] data = new byte[4096];
        EChatEntryType chatEntryType;

        int bytesRead = SteamMatchmaking.GetLobbyChatEntry(
            lobbyID,
            (int)callback.m_iChatID,
            out senderID,
            data,
            data.Length,
            out chatEntryType
        );

        if (bytesRead > 0)
        {
            string message = System.Text.Encoding.UTF8.GetString(data, 0, bytesRead - 1);
            string senderName = SteamFriends.GetFriendPersonaName(senderID);
            CSteamID mySteamID = SteamUser.GetSteamID();
            string formattedMessage = $"<i>{senderName}</i>: {message}";

            if (mySteamID != senderID)
            {
                formattedMessage = $"<color=yellow><i>{senderName}</i>: {message}</color>";
            }

            if (textChatDisplay != null)
            {
                if (string.IsNullOrEmpty(textChatDisplay.text))
                {
                    textChatDisplay.text = formattedMessage;
                }
                else
                {
                    textChatDisplay.text += $"\n{formattedMessage}";
                }

                ScrollToBottom();
            }

            OnChatMessageReceived?.Invoke(senderID, message);
        }
    }

    private void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomCoroutine());
    }
    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        Canvas.ForceUpdateCanvases();

        // 0f — top, 1f — bottom
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    private void OnInputSubmit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            sendButton_Click();
            StartCoroutine(ActivateInputNextFrame());
        }
    }

    private IEnumerator ActivateInputNextFrame()
    {
        yield return null;
        inputFieldMessage.ActivateInputField();
        inputFieldMessage.Select();
    }

    private void OnDestroy()
    {
        sendButton.onClick.RemoveListener(sendButton_Click);
        inputFieldMessage.onSubmit.RemoveListener(OnInputSubmit);
    }
}