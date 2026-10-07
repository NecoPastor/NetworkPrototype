using Game.Systems;
using Game.Systems.Service;
using UnityEngine;

public class ServiceBootOverDispatcher : MonoBehaviour
{
    public void CallServiceBootOver()
    {
        if (ServiceLocator.TryGetService(out GlobalEventHub globalEventHub))
            globalEventHub.EventBus.Publish(new ServiceBootOverEvent());
    }
}