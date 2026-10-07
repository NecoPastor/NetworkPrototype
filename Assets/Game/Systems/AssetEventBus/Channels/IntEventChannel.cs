using UnityEngine;

namespace Game.Systems.AssetEventBus
{
    [CreateAssetMenu(fileName = "IntEventChannel", menuName = "Game/Systems/AssetEventBus/Channels/Int Event Channel")]
    public class IntEventChannel : EventChannel<int> { }
}