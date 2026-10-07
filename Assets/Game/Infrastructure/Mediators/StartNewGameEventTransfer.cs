using Game.Systems.GameStateMachine;
using Game.Systems.MainMenu;
using Game.Systems.Service;
using UnityEngine;

public class StartNewGameEventTransfer : MonoBehaviour
{
    [SerializeField] private StartNewGameAction startNewGameAction;

    private void OnEnable()
    {
        if (startNewGameAction != null)
            startNewGameAction.OnExecute += HandleStartNewGame;
    }

    private void OnDisable()
    {
        if (startNewGameAction != null)
            startNewGameAction.OnExecute -= HandleStartNewGame;
    }

    private void HandleStartNewGame()
    {
        if (ServiceLocator.TryGetService(out StateMachine stateMachine))
        {
            stateMachine.SwitchState<Playing>();
            Debug.Log("[Mediator] Game started via StateMachine");
        }
        else
        {
            Debug.LogWarning("[Mediator] StateMachine not found in ServiceLocator");
        }
    }
}
