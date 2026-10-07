using Game.Systems;
using Game.Systems.GameStateMachine;
using UnityEngine;

public class GameStateEventPublisher : MonoBehaviour
{
    [Header("Источники")]
    [Tooltip("Компонент, отслеживающий состояние игры")]
    [SerializeField] private StateMachine _stateMachine;

    [Tooltip("Глобальный хаб событий")]
    [SerializeField] private GlobalEventHub _eventHub;

    private void Awake()
    {
        if (_stateMachine == null)
            Debug.LogError("[GameStateEventPublisher] GameStateMachine не назначен");

        if (_eventHub == null)
            Debug.LogError("[GameStateEventPublisher] GlobalEventHub не назначен");

        _stateMachine.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (_stateMachine != null)
            _stateMachine.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previous, GameState current)
    {
        //var gameEvent = new GameStateChangedEvent(previous, current);
        //_eventHub.EventBus.Publish(gameEvent);
    }
}
