using System;
using System.Collections.Generic;

namespace Game.Systems
{
    /// <summary>
    /// Централизованный EventBus с безопасными подписками/отписками
    /// и защитой от модификации коллекций во время публикации.
    /// </summary>
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> subscribers = new();

        private readonly List<Action> deferredSubscribes = new();
        private readonly List<Action> deferredUnsubscribes = new();

        private bool isPublishing = false;

        public Dictionary<Type, List<Delegate>> GetAllSubscribers() => subscribers;

        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (isPublishing)
            {
                deferredSubscribes.Add(() =>
                {
                    if (!subscribers.ContainsKey(eventType))
                        subscribers[eventType] = new List<Delegate>();

                    if (!subscribers[eventType].Contains(handler))
                        subscribers[eventType].Add(handler);
                });

                return;
            }

            if (!subscribers.ContainsKey(eventType))
                subscribers[eventType] = new List<Delegate>();

            if (!subscribers[eventType].Contains(handler))
                subscribers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (isPublishing)
            {
                deferredUnsubscribes.Add(() =>
                {
                    if (subscribers.ContainsKey(eventType))
                        subscribers[eventType].Remove(handler);
                });

                return;
            }

            if (subscribers.ContainsKey(eventType))
                subscribers[eventType].Remove(handler);
        }

        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (!subscribers.TryGetValue(eventType, out var list) || list.Count == 0)
                return;

            isPublishing = true;

            // Быстрее foreach, нет Enumerator, нет исключений
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is Action<T> action)
                    action.Invoke(gameEvent);
            }

            isPublishing = false;

            // Выполняем отложенные подписки
            if (deferredSubscribes.Count > 0)
            {
                for (int i = 0; i < deferredSubscribes.Count; i++)
                    deferredSubscribes[i].Invoke();

                deferredSubscribes.Clear();
            }

            // Выполняем отложенные отписки
            if (deferredUnsubscribes.Count > 0)
            {
                for (int i = 0; i < deferredUnsubscribes.Count; i++)
                    deferredUnsubscribes[i].Invoke();

                deferredUnsubscribes.Clear();
            }
        }

        public void Clear()
        {
            subscribers.Clear();
            deferredSubscribes.Clear();
            deferredUnsubscribes.Clear();
        }
    }
}
