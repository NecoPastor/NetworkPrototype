using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.Service
{
    public class ServiceLocator : MonoBehaviour
    {
        public static ServiceLocator Instance => instance;
        private static ServiceLocator instance;

        /// <summary>
        /// Событие, вызываемое при регистрации нового сервиса.
        /// </summary>
        public event Action<object> EventOnServiceRegistered;

        /// <summary>
        /// Внутреннее хранилище сервисов, сопоставленных по типу.
        /// </summary>
        private Dictionary<Type, object> services;

        public void Initialize()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[ServiceLocator] Duplicate instance detected. Destroying self.");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            services = new Dictionary<Type, object>();
        }

        public static void RegisterService(Type type, object obj)
        {
            if (instance == null)
            {
                Debug.LogWarning("[ServiceLocator] Instance is not initialized.");
                return;
            }

            instance.Register(type, obj);
        }

        private void Register(Type type, object instance)
        {
            if (services.ContainsKey(type))
            {
                //Debug.LogWarning($"[ServiceLocator] Сервис {type.Name} уже зарегистрирован.");
                return;
            }

            services[type] = instance;
            EventOnServiceRegistered?.Invoke(instance);
            //Debug.Log($"[ServiceLocator] Зарегистрирован сервис: {type.Name}");
        }

        public static void RemoveService<T>(T service)
        {
            if (instance == null)
            {
                Debug.LogWarning("[ServiceLocator] Instance is not initialized.");
                return;
            }

            instance.Remove(service);
        }

        private void Remove<T>(T service)
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var existing) && existing == (object)service)
            {
                services.Remove(type);
                Debug.Log($"[ServiceLocator] Удалён сервис: {type.Name}");

                if (service is IService s)
                    s.RemoveService();
            }
        }

        public static bool TryGetService<T>(out T service) where T : class
        {
            service = null;

            if (instance == null)
            {
                Debug.LogWarning("[ServiceLocator] Instance is not initialized.");
                return false;
            }

            return instance.GetServiceProvider(out service);
        }

        private bool GetServiceProvider<T>(out T service) where T : class
        {
            service = null;

            foreach (var entry in services.Values)
            {
                if (entry is IServiceProvider<T> provider)
                {
                    service = provider.Get();
                    return true;
                }
            }

            return false;
        }
    }
}