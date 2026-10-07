using Game.Systems.Service;
using UnityEngine;
using UnityEngine.Events;

public class ServiceBootstrapper : MonoBehaviour
{
    [SerializeField] private float loadOverDelay = 1;
    [SerializeField] private ServiceAssets serviceAssets;
    [SerializeField] private UnityEvent eventBootOver;

    private void Start()
    {
        if (ServiceLocator.Instance == null)
        {
            var go = new GameObject();
            go.name = "ServiceLocator";
            var serviceLocator = go.AddComponent<ServiceLocator>();
            serviceLocator.Initialize();
        }

        foreach (var prefab in serviceAssets.servicePrefabs)
        {
            GameObject instance;

            var serviceType = ServiceUtils.GetProvidedType(prefab);
            if (serviceType == null)
            {
                Debug.LogWarning($"Prefab {prefab.name} не реализует IServiceProvider<>");
                continue;
            }

            // Проверяем наличие сервиса на сцене
            if (ServiceUtils.TryGetService(serviceType, out var existingService))
            {
                instance = existingService.gameObject;
            }
            else
            {
                instance = Instantiate(prefab.gameObject);
            }

            // Инициализация всех систем
            var systems = instance.GetComponents<IGameSystem>();
            foreach (var system in systems)
                system.Initialize();
        }

        //foreach (var prefab in serviceAssets.servicePrefabs)
        //{
        //    var gameSystem = prefab.GetComponents<IGameSystem>();
        //    GameObject instance = null;

        //    if (ServiceUtils.GetProvidedType(gameSystem, out var existService))
        //    {
        //        instance = existService;
        //    }
        //    else
        //    {
        //        instance = Instantiate(prefab);
        //    }

        //    var systems = instance.GetComponents<IGameSystem>();
        //    foreach (var system in systems)
        //        system.Initialize();
        //}

        Invoke(nameof(CreateServices), loadOverDelay);
    }

    protected virtual void CreateServices()
    {
        eventBootOver.Invoke();
        Destroy(this);
    }
}