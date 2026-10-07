using UnityEngine;

namespace Game.Systems.AssetEventBus
{
    [CreateAssetMenu(fileName = "FloatEventChannel", menuName = "Game/Systems/AssetEventBus/Channels/Float Event Channel")]
    public class FloatEventChannelSO : EventChannelSO<float> { }
}