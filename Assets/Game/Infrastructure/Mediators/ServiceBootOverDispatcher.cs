using Game.Systems.AssetEventBus;
using Game.Systems.GameStateMachine;
using Game.Systems.Service;
using UnityEngine;

public class ServiceBootOverDispatcher : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private VoidEventChannel onServicesBootstrapped;

    [GameState]
    [SerializeField] private string nextGameState;

    private void OnEnable()
    {
        onServicesBootstrapped.Subscribe(HandleServicesBootstrapped);
    }

    private void OnDisable()
    {
        onServicesBootstrapped.Unsubscribe(HandleServicesBootstrapped);
    }

    public void HandleServicesBootstrapped()
    {
        if (string.IsNullOrEmpty(nextGameState)) return;
        if (nextGameState == "Null" || nextGameState == "None") return;

        if (ServiceLocator.TryGetService(out StateMachine stateMachine))
            //stateMachine.SwitchState<Lobby>();
            stateMachine.SwitchState(nextGameState);
    }
}