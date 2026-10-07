using Game.Systems;
using Game.Systems.GameStateMachine;
using Game.Systems.Service;
using System.Collections;
using UnityEngine;

public class GameEntryPoint : MonoBehaviour
{
    private GlobalEventHub globalEventHub;

    private void Start()
    {
        StartCoroutine(WaitForBootOver());
    }

    private IEnumerator WaitForBootOver()
    {
        GlobalEventHub globalEventHub = null;

        while (!ServiceLocator.TryGetService(out globalEventHub))
            yield return null;

        this.globalEventHub = globalEventHub;
        this.globalEventHub.EventBus.Subscribe<ServiceBootOverEvent>(HandleBootOver);
    }

    private void HandleBootOver(ServiceBootOverEvent _)
    {
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        if (globalEventHub != null)
            globalEventHub.EventBus.Unsubscribe<ServiceBootOverEvent>(HandleBootOver);

        if (ServiceLocator.TryGetService(out StateMachine stateMachine))
            stateMachine.SwitchState<MainMenu>();
    }

}
