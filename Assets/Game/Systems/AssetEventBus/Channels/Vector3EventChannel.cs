using UnityEngine;

namespace Game.Systems.AssetEventBus
{
    [CreateAssetMenu(fileName = "Vector3EventChannel", menuName = "Game/Systems/AssetEventBus/Channels/Vector3 Event Channel")]
    public class Vector3EventChannel : EventChannel<Vector3> { }
}