using Game.Systems.AssetEventBus;
using Game.Systems.Service;
using UnityEngine;

public class ServiceBootstrapper : MonoBehaviour
{
    [SerializeField, Range(1, 60)] private float loadOverDelay = 1;
    [SerializeField] private ServiceAssets serviceAssets;

    [Header("Event Channels")]
    [SerializeField] private VoidEventChannel onServicesBootstrapped;

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

        Invoke(nameof(CreateServices), loadOverDelay);
    }

    protected virtual void CreateServices()
    {
        onServicesBootstrapped?.RaiseEvent();
        Destroy(this);
    }
}