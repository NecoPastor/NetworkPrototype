using Steamworks;
using UnityEngine;

namespace Game.Systems
{
    [DisallowMultipleComponent]
    public class SteamStatusViewer : MonoBehaviour
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
        [SerializeField] private KeyCode _toggleKey = KeyCode.F3;
        [SerializeField] private WindowAnchor _anchor = WindowAnchor.TopLeft;
        [SerializeField] private Rect _guiRect = new Rect(10, 10, 260, 240);
        [SerializeField] private bool _allowDraggable = true;

        [Header("Appearance Customization")]
        [SerializeField] private Color _backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private Color _headerColor = new Color(0.4f, 0.7f, 1f);

        [Header("References")]
        [SerializeField] private SteamNetworkManager _networkManager;

        private Texture2D _backgroundTexture;
        private GUIStyle _windowBoxStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;

        private void Awake()
        {
            if (_networkManager == null)
            {
                _networkManager = FindFirstObjectByType<SteamNetworkManager>();
            }

            UpdateWindowPosition();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                UpdateWindowPosition();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                _showGUI = !_showGUI;
            }

            if (_networkManager == null)
            {
                _networkManager = FindFirstObjectByType<SteamNetworkManager>();
            }
        }

        private void OnGUI()
        {
            if (!_showGUI) return;

            InitStylesIfNeeded();

            _guiRect = GUI.Window(1, _guiRect, DrawSteamMonitor, "", _windowBoxStyle);
        }

        private void DrawSteamMonitor(int windowID)
        {
            GUILayout.Space(2);
            GUILayout.Label("<b>Steam Monitor</b>", _headerStyle);
            GUILayout.Space(4);

            if (_networkManager == null)
            {
                GUILayout.Label("Manager: <color=#FF5555>Not Found</color>", _labelStyle);
            }
            else if (!_networkManager.IsInitialized)
            {
                GUILayout.Label("Steam API: <color=#FF5555>Not Initialized</color>", _labelStyle);
            }
            else
            {
                // Статус клиента Steam
                bool isLogged = SteamUser.BLoggedOn();
                string statusColor = isLogged ? "#00FF00" : "#FFFF00";
                GUILayout.Label($"Status: <color={statusColor}>{(isLogged ? "Online" : "Offline")}</color>", _labelStyle);

                // Данные игрока
                string playerName = SteamFriends.GetPersonaName();
                CSteamID steamId = SteamUser.GetSteamID();
                GUILayout.Label($"Player: <b>{playerName}</b>", _labelStyle);
                GUILayout.Label($"SteamID: <size=10>{steamId}</size>", _labelStyle);

                GUILayout.Space(4);

                // Целевой хост
                CSteamID targetHost = _networkManager.TargetHostSteamID;
                string hostColor = targetHost != CSteamID.Nil ? "#00FF00" : "#AAAAAA";
                GUILayout.Label($"Target Host: <color={hostColor}>{(targetHost != CSteamID.Nil ? targetHost.ToString() : "None")}</color>", _labelStyle);

                GUILayout.Space(6);

                // Данные о Лобби
                CSteamID lobbyId = _networkManager.CurrentLobbyID;
                if (lobbyId != CSteamID.Nil && lobbyId.IsValid())
                {
                    CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(lobbyId);
                    string ownerName = SteamFriends.GetFriendPersonaName(lobbyOwner);
                    int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
                    int maxMembers = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);

                    GUILayout.Label("<color=#66CCFF><b>Lobby Info</b></color>", _labelStyle);
                    GUILayout.Label($"ID: <size=10>{lobbyId}</size>", _labelStyle);
                    GUILayout.Label($"Owner: <b>{ownerName}</b>", _labelStyle);
                    GUILayout.Label($"Players: <b>{memberCount} / {maxMembers}</b>", _labelStyle);

                    GUILayout.Space(2);
                    GUILayout.Label("<b>Members:</b>", _labelStyle);
                    for (int i = 0; i < memberCount; i++)
                    {
                        CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
                        string memberName = SteamFriends.GetFriendPersonaName(memberId);
                        string role = (memberId == lobbyOwner) ? " <color=#FFD700>(Host)</color>" : "";
                        GUILayout.Label($" • {memberName}{role}", _labelStyle);
                    }
                }
                else
                {
                    GUILayout.Label("Lobby: <color=#AAAAAA>NotInLobby</color>", _labelStyle);
                }
            }

            if (_allowDraggable)
            {
                GUI.DragWindow(new Rect(0, 0, 10000, 10000));
            }
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
}