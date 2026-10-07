using UnityEngine;

namespace Game.Systems.AssetEventBus
{
    [CreateAssetMenu(fileName = "BoolEventChannel", menuName = "Game/Systems/AssetEventBus/Channels/Bool Event Channel")]
    public class BoolEventChannelSO : EventChannelSO<bool> { }
}