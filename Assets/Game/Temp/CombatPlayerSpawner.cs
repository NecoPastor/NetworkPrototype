using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class CombatPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Subscribe to remote connection state changes
        ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;

        // Spawn for already connected clients (like host)
        foreach (var conn in ServerManager.Clients.Values)
        {
            SpawnPlayerForConnection(conn);
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (ServerManager != null)
        {
            ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        }
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (!base.IsServer) return;

        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            SpawnPlayerForConnection(conn);
        }
    }

    private void SpawnPlayerForConnection(NetworkConnection conn)
    {
        foreach (var obj in conn.Objects)
        {
            if (obj != null && obj.gameObject.name.Contains(playerPrefab.gameObject.name))
            {
                return;
            }
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        NetworkObject spawnedInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);

        ServerManager.Spawn(spawnedInstance, conn);
        Debug.Log($"[CombatPlayerSpawner] Spawned player for Client ID: {conn.ClientId}");
    }
}