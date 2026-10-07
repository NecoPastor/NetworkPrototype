using System;
using UnityEngine;

namespace Game.Systems.AssetEventBus
{
    /// <summary>
    /// Абстрактный ScriptableObject-канал для передачи данных типа T через инспектор Unity.
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject
    {
        private Action<T> _onEventRaised;

        /// <summary>
        /// Вызвать событие и передать значение всем подписчикам канала.
        /// </summary>
        public void RaiseEvent(T value)
        {
            _onEventRaised?.Invoke(value);
        }

        /// <summary>
        /// Подписаться на данный канал.
        /// </summary>
        public void Subscribe(Action<T> action)
        {
            _onEventRaised += action;
        }

        /// <summary>
        /// Отписаться от данного канала.
        /// </summary>
        public void Unsubscribe(Action<T> action)
        {
            _onEventRaised -= action;
        }
    }
}