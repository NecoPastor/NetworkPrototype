using Game.Systems.Service;
using UnityEngine;

namespace Game.Systems
{
    public class NetworkManagerProvider : GameSystemProvider, IServiceProvider<NetworkManager>
    {
        [SerializeField] private NetworkManager networkManager;

        public NetworkManager Get() => networkManager;
    }
}