using FishNet.Object;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Регистрируем объект сцены для сети
        // (для объектов сцены Spawn вызывать не нужно, достаточно того, что он уже здесь)
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsClient && !base.IsServer)
        {
            CmdRequestOwnership();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdRequestOwnership()
    {
        base.GiveOwnership(base.Owner);
    }

    private void Update()
    {
        if (base.NetworkObject == null || !base.IsOwner) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(moveX, moveY, 0f).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}