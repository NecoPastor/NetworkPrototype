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
    public class VoidEventChannel : ScriptableObject
    {

        private Action _onEventRaised;

        /// <summary>
        /// Вызвать событие и передать значение всем подписчикам канала.
        /// </summary>
        public void RaiseEvent()
        {
            _onEventRaised?.Invoke();
        }

        /// <summary>
        /// Подписаться на данный канал.
        /// </summary>
        public void Subscribe(Action action)
        {
            _onEventRaised += action;
        }

        /// <summary>
        /// Отписаться от данного канала.
        /// </summary>
        public void Unsubscribe(Action action)
        {
            _onEventRaised -= action;
        }
    }
}