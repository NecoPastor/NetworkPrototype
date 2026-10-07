using Game.Systems.Service;
using UnityEngine;

namespace Game.Systems
{
    public class GlobalEventHubProvider : GameSystemProvider, IServiceProvider<GlobalEventHub>
    {
        [SerializeField] private GlobalEventHub globalEventHub;

        public GlobalEventHub Get() => globalEventHub;

    }
}
