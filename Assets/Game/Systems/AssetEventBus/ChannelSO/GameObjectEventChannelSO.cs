using UnityEngine;

namespace Game.Systems.AssetEventBus
{
    [CreateAssetMenu(fileName = "GameObjectEventChannel", menuName = "Game/Systems/AssetEventBus/Channels/GameObject Event Channel")]
    public class GameObjectEventChannelSO : EventChannelSO<GameObject> { }
}