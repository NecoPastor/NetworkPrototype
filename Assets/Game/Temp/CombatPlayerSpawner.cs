using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

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
        if (!base.IsServer) return;

        bool targetSceneLoaded = false;
        if (args.LoadedScenes != null)
        {
            foreach (var scene in args.LoadedScenes)
            {
                if (scene.name == combatSceneName)
                {
                    targetSceneLoaded = true;
                    break;
                }
            }
        }

        if (!targetSceneLoaded) return;

        if (args.QueueData.Connections != null)
        {
            foreach (NetworkConnection conn in args.QueueData.Connections)
            {
                SpawnPlayer(conn);
            }
        }
    }

    private void SpawnPlayer(NetworkConnection conn)
    {
        if (conn == null || !conn.IsAuthenticated) return;

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
        ServerManager.Spawn(playerInstance, conn);
    }
}