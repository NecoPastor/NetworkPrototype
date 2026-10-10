using Game.Systems.Service;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Systems.SteamNetwork
{
    public class StartGameMessageProcessor : MonoBehaviour
    {
        [SerializeField] private LobbyNetworkManager lobby;

        private SteamNetworkManager _networkService;
        private bool _isGameStarted;

        private void Start()
        {
            if (ServiceLocator.TryGetService(out SteamNetworkManager service))
            {
                _networkService = service;
            }

            if (_networkService == null)
            {
                Debug.LogError("[StartGameMessageProcessor] SteamNetworkManager service is missing.");
                return;
            }

            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            if (_networkService != null)
            {
                // Регистрируем обработчик для клиентов, ожидающих сигнал старта
                _networkService.RegisterLobbyHandler<StartGameMessage>(OnStartGameReceived, StartGameMessage.Deserialize);
            }
        }

        private void UnregisterHandlers()
        {
            if (_networkService != null)
            {
                _networkService.UnregisterLobbyHandler<StartGameMessage>();
            }
        }

        /// <summary>
        /// Публичный метод для привязки к UI-кнопке «Начать игру» у Хоста
        /// </summary>
        public void TryStartGame()
        {
            if (_networkService == null)
            {
                Debug.LogError("[StartGameMessageProcessor] Cannot start game: SteamNetworkManager is missing.");
                return;
            }

            if (lobby == null)
            {
                Debug.LogError("[StartGameMessageProcessor] Cannot start game: LobbyNetworkManager reference is missing.");
                return;
            }

            CSteamID currentLobby = lobby.CurrentLobbyID;
            if (!currentLobby.IsValid())
            {
                Debug.LogWarning("[StartGameMessageProcessor] Cannot start game: Invalid Lobby ID.");
                return;
            }

            // 1. Проверяем, является ли текущий пользователь владельцем лобби
            CSteamID hostId = SteamMatchmaking.GetLobbyOwner(currentLobby);
            CSteamID localUserId = SteamUser.GetSteamID();

            if (hostId != localUserId)
            {
                Debug.LogWarning("[StartGameMessageProcessor] Only the host can start the game.");
                return;
            }

            // 2. Синхронизируем ID лобби с сетевым сервисом
            _networkService.SetLobby(currentLobby);

            // 3. Создаем сообщение
            StartGameMessage message = new StartGameMessage
            {
                MapId = 0,
                RandomSeed = 0
            };

            // 4. Отправляем через сетевой сервис
            bool success = _networkService.SendLobbyMessage(message, writer => StartGameMessage.Serialize(message, writer));

            if (success)
            {
                Debug.Log($"[StartGameMessageProcessor] Game start signal sent successfully! Map: {message.MapId}, Seed: {message.RandomSeed}");

                // Закрываем лобби для новых игроков перед началом
                SteamMatchmaking.SetLobbyJoinable(currentLobby, false);

                // Хост также запускает загрузку сцены
                TriggerGameLoad(message.MapId, message.RandomSeed);
            }
            else
            {
                Debug.LogError("[StartGameMessageProcessor] Failed to send game start message via SteamNetworkManager.");
            }
        }

        private void OnStartGameReceived(CSteamID senderID, StartGameMessage message)
        {
            // Защита от повторного запуска
            if (_isGameStarted) return;

            // Проверяем, что сообщение пришло именно от хоста лобби
            if (lobby != null && lobby.CurrentLobbyID.IsValid())
            {
                CSteamID hostId = SteamMatchmaking.GetLobbyOwner(lobby.CurrentLobbyID);
                if (senderID != hostId)
                {
                    Debug.LogWarning($"[StartGameMessageProcessor] Ignored StartGameMessage from non-host user: {senderID}");
                    return;
                }
            }

            Debug.Log($"[StartGameMessageProcessor] Game start received from host! MapId: {message.MapId}, RandomSeed: {message.RandomSeed}");
            TriggerGameLoad(message.MapId, message.RandomSeed);
        }

        private void TriggerGameLoad(int mapId, int randomSeed)
        {
            if (_isGameStarted) return;
            _isGameStarted = true;

            // Здесь при необходимости сохраняются параметры матча перед загрузкой сцены
            StartCoroutine(LoadSceneRoutine());
        }

        private System.Collections.IEnumerator LoadSceneRoutine()
        {
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync("Combat");

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            Debug.Log("[StartGameMessageProcessor] Game scene successfully loaded.");
        }

        private void OnDisable()
        {
            UnregisterHandlers();
        }

        private void OnDestroy()
        {
            UnregisterHandlers();
        }
    }
}