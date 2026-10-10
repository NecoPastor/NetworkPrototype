using Game.Systems.Service;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Systems.SteamNetwork
{
    public class LobbyGameStarter : MonoBehaviour
    {
        [SerializeField] private LobbyNetworkManager lobby;

        private SteamNetworkManager _networkService;

        private void Start()
        {
            if (ServiceLocator.TryGetService(out SteamNetworkManager service))
            {
                _networkService = service;
            }

            if (_networkService == null)
            {
                Debug.LogWarning("[LobbyGameStarter] SteamNetworkManager is not available.");
            }
        }

        /// <summary>
        /// Публичный метод для привязки к UI-кнопке «Начать игру» у Хоста
        /// </summary>
        public void TryStartGame()
        {
            if (_networkService == null)
            {
                Debug.LogError("[LobbyGameStarter] Cannot start game: SteamNetworkManager is missing.");
                return;
            }

            if (lobby == null)
            {
                Debug.LogError("[LobbyGameStarter] Cannot start game: LobbyNetworkManager reference is missing.");
                return;
            }

            CSteamID currentLobby = lobby.CurrentLobbyID;
            if (!currentLobby.IsValid())
            {
                Debug.LogWarning("[LobbyGameStarter] Cannot start game: Invalid Lobby ID.");
                return;
            }

            // 1. Проверяем, является ли текущий пользователь владельцем лобби
            CSteamID hostId = SteamMatchmaking.GetLobbyOwner(currentLobby);
            CSteamID localUserId = SteamUser.GetSteamID();

            if (hostId != localUserId)
            {
                Debug.LogWarning("[LobbyGameStarter] Only the host can start the game.");
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
                Debug.Log($"[LobbyGameStarter] Game start signal sent successfully! Map: {0}, Seed: {0}");

                // Закрываем лобби для новых игроков, чтобы никто лишний не зашел перед началом
                SteamMatchmaking.SetLobbyJoinable(currentLobby, false);
                LoadGameScene();
            }
            else
            {
                Debug.LogError("[LobbyGameStarter] Failed to send game start message via SteamNetworkManager.");
            }
        }


        /// <summary>
        /// Запуск загрузки боевой сцены на стороне хоста
        /// </summary>
        private void LoadGameScene()
        {
            // Загружаем сцену асинхронно (можно указать имя сцены или её индекс в Build Settings)
            SceneManager.LoadSceneAsync("Combat"); // Замени "GameSceneName" на имя твоей боевой сцены
        }
    }
}