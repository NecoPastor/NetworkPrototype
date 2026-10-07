using UnityEngine;

namespace Game.Systems.AssetEventBus
{
    [CreateAssetMenu(fileName = "StringEventChannel", menuName = "Game/Systems/AssetEventBus/Channels/String Event Channel")]
    public class StringEventChannelSO : EventChannelSO<string> { }
}