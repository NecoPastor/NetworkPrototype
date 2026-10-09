using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

[DisallowMultipleComponent]
public class FishNetStatusViewer : MonoBehaviour
{
    public enum WindowAnchor
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        CustomPosition
    }

    [Header("GUI Settings")]
    [SerializeField] private bool _showGUI = true;
    [SerializeField] private WindowAnchor _anchor = WindowAnchor.TopLeft;
    [SerializeField] private Rect _guiRect = new Rect(10, 10, 220, 130);
    [SerializeField] private bool _allowDraggable = true;

    [Header("Appearance Customization")]
    [SerializeField] private Color _backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);
    [SerializeField] private Color _textColor = Color.white;
    [SerializeField] private Color _headerColor = new Color(0.4f, 0.7f, 1f);

    [Header("Inspector Status (Read Only)")]
    [SerializeField] private string _serverStatus = "Not Active";
    [SerializeField] private string _clientStatus = "Not Active";
    [SerializeField] private int _connectedClientsCount = 0;
    [SerializeField] private int _pingMs = 0;

    private NetworkManager _networkManager;
    private Texture2D _backgroundTexture;
    private GUIStyle _windowBoxStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _headerStyle;

    private void Awake()
    {
        _networkManager = InstanceFinder.NetworkManager;
        UpdateWindowPosition();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            UpdateWindowPosition();
        }
    }

    private void OnEnable()
    {
        if (_networkManager == null) return;
        _networkManager.ServerManager.OnServerConnectionState += OnServerStateChanged;
        _networkManager.ClientManager.OnClientConnectionState += OnClientStateChanged;
    }

    private void OnDisable()
    {
        if (_networkManager == null) return;
        _networkManager.ServerManager.OnServerConnectionState -= OnServerStateChanged;
        _networkManager.ClientManager.OnClientConnectionState -= OnClientStateChanged;
    }

    private void Update()
    {
        if (_networkManager == null)
            _networkManager = InstanceFinder.NetworkManager;

        if (_networkManager == null) return;

        bool isServerStarted = _networkManager.IsServerStarted;
        bool isClientStarted = _networkManager.IsClientStarted;

        _serverStatus = isServerStarted ? "Running" : "Stopped";
        _clientStatus = isClientStarted ? "Connected" : "Disconnected";

        _connectedClientsCount = isServerStarted ? _networkManager.ServerManager.Clients.Count : 0;
        _pingMs = isClientStarted ? (int)_networkManager.TimeManager.RoundTripTime : 0;
    }

    private void OnServerStateChanged(ServerConnectionStateArgs args)
    {
        _serverStatus = args.ConnectionState.ToString();
    }

    private void OnClientStateChanged(ClientConnectionStateArgs args)
    {
        _clientStatus = args.ConnectionState.ToString();
    }

    private void UpdateWindowPosition()
    {
        float margin = 10f;
        switch (_anchor)
        {
            case WindowAnchor.TopLeft:
                _guiRect.x = margin;
                _guiRect.y = margin;
                break;
            case WindowAnchor.TopRight:
                _guiRect.x = Screen.width - _guiRect.width - margin;
                _guiRect.y = margin;
                break;
            case WindowAnchor.BottomLeft:
                _guiRect.x = margin;
                _guiRect.y = Screen.height - _guiRect.height - margin;
                break;
            case WindowAnchor.BottomRight:
                _guiRect.x = Screen.width - _guiRect.width - margin;
                _guiRect.y = Screen.height - _guiRect.height - margin;
                break;
            case WindowAnchor.CustomPosition:
                break;
        }
    }

    private void OnGUI()
    {
        if (!_showGUI || _networkManager == null) return;

        InitStylesIfNeeded();

        _guiRect = GUI.Window(0, _guiRect, DrawNetworkMonitor, "", _windowBoxStyle);
    }

    private void DrawNetworkMonitor(int windowID)
    {
        GUILayout.Space(2);
        GUILayout.Label("<b>FishNet Monitor</b>", _headerStyle);
        GUILayout.Space(4);

        // Статус Сервера
        string serverColor = _networkManager.IsServerStarted ? "#00FF00" : "#FF5555";
        GUILayout.Label($"Server: <color={serverColor}>{_serverStatus}</color>", _labelStyle);
        if (_networkManager.IsServerStarted)
        {
            GUILayout.Label($"Clients: <b>{_connectedClientsCount}</b>", _labelStyle);
        }

        GUILayout.Space(4);

        // Статус Клиента
        string clientColor = _networkManager.IsClientStarted ? "#00FF00" : "#FF5555";
        GUILayout.Label($"Client: <color={clientColor}>{_clientStatus}</color>", _labelStyle);
        if (_networkManager.IsClientStarted)
        {
            GUILayout.Label($"Ping: <b>{_pingMs} ms</b>", _labelStyle);
        }

        if (_allowDraggable)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }

    private void InitStylesIfNeeded()
    {
        if (_backgroundTexture == null)
        {
            _backgroundTexture = MakeTex(2, 2, _backgroundColor);
        }

        if (_windowBoxStyle == null)
        {
            _windowBoxStyle = new GUIStyle(GUI.skin.window);
            _windowBoxStyle.normal.background = _backgroundTexture;
            _windowBoxStyle.onNormal.background = _backgroundTexture;
            _windowBoxStyle.padding = new RectOffset(8, 8, 6, 8);
        }

        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = 12,
                padding = new RectOffset(0, 0, 0, 0),
                normal = { textColor = _textColor }
            };
        }

        if (_headerStyle == null)
        {
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = _headerColor }
            };
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}