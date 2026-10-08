using Game.Systems;
using System;
using System.Linq;
using UnityEngine;

public static class ServiceUtils
{
    /// <summary>
    /// Получает тип T из компонента, реализующего IServiceProvider<T>.
    /// </summary>
    public static Type GetProvidedType(MonoBehaviour component)
    {
        var iface = component.GetType().GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == typeof(IServiceProvider<>));

        return iface?.GetGenericArguments()[0];
    }

    /// <summary>
    /// Пытается найти на сцене сервис указанного типа.
    /// </summary>
    public static bool TryGetService(Type serviceType, out MonoBehaviour service)
    {
        service = GameObject.FindObjectsOfType<MonoBehaviour>()
            .FirstOrDefault(mb => mb.GetType().GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == typeof(IServiceProvider<>) &&
                          i.GetGenericArguments()[0] == serviceType));

        return service != null;
    }
}