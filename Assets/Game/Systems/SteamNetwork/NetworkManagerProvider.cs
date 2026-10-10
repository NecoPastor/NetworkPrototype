using Game.Systems.Service;
using Game.Systems.SteamNetwork;
using UnityEngine;

namespace Game.Systems
{
    public class NetworkManagerProvider : GameSystemProvider, IServiceProvider<SteamNetworkManager>
    {
        [SerializeField] private SteamNetworkManager networkManager;

        public SteamNetworkManager Get() => networkManager;
    }
}