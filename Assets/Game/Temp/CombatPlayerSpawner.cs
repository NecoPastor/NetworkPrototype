using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private string combatSceneName = "Combat";

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (SceneManager != null)
        {
            // Подписываемся на стандартное событие FishNet
            SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (SceneManager != null)
        {
            SceneManager.OnLoadEnd -= SceneManager_OnLoadEnd;
        }
    }

    private void SceneManager_OnLoadEnd(SceneLoadEndEventArgs args)
    {
        // Выполняем только на сервере
        if (!IsServer) return;

        // 1. Проверяем, есть ли среди загруженных сцен наша боевая сцена
        bool isCombatScene = false;
        Scene loadedCombatScene = default;

        if (args.LoadedScenes != null)
        {
            foreach (Scene scene in args.LoadedScenes)
            {
                if (scene.name == combatSceneName)
                {
                    isCombatScene = true;
                    loadedCombatScene = scene;
                    break;
                }
            }
        }

        if (!isCombatScene) return;

        // Берём все текущие клиентские подключения на сервере
        var connections = ServerManager.Clients;

        if (connections != null && connections.Count > 0)
        {
            foreach (var pair in connections)
            {
                NetworkConnection conn = pair.Value;
                if (conn != null && conn.IsActive)
                {
                    SpawnPlayer(conn, loadedCombatScene);
                }
            }
        }
    }

    private void SpawnPlayer(NetworkConnection conn, Scene targetScene)
    {
        if (conn == null || !conn.IsAuthenticated) return;

        // Проверяем, не заспавнен ли уже игрок у этого подключения
        foreach (var obj in conn.Objects)
        {
            if (obj != null && obj.gameObject.name.Contains(playerPrefab.gameObject.name))
            {
                return;
            }
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        NetworkObject playerInstance = Instantiate(playerPrefab, position, rotation);

        // ВАЖНО: передаем targetScene 3-м параметром, чтобы FishNet сразу привязал объект к нужной сцене
        ServerManager.Spawn(playerInstance, conn, targetScene);
    }

    private void OnDestroy()
    {
        if (SceneManager != null)
        {
            SceneManager.OnLoadEnd -= SceneManager_OnLoadEnd;
        }
    }
}