using System;
using UnityEngine;

namespace Game.Systems.AssetEventBus
{
    /// <summary>
    /// Канал для простых событий без передаваемых параметров (сигнал/триггер).
    /// </summary>
    [CreateAssetMenu(
        fileName = "VoidEventChannel",
        menuName = "Game/Systems/AssetEventBus/Channels/Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        private Action _onEventRaised;

        public void RaiseEvent()
        {
            _onEventRaised?.Invoke();
        }

        public void Subscribe(Action action)
        {
            _onEventRaised += action;
        }

        public void Unsubscribe(Action action)
        {
            _onEventRaised -= action;
        }
    }
}