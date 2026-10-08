using Game.Systems.Service;
using UnityEngine;
using NetManager = FishNet.Managing.NetworkManager;

namespace Game.Systems
{
    public class FishNetworkManagerProvider : GameSystemProvider, IServiceProvider<NetManager>
    {
        [SerializeField] private NetManager networkManager;

        public NetManager Get() => networkManager;
    }
}