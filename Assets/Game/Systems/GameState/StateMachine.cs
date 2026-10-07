using UnityEngine;

namespace Game.Systems.GameStateMachine
{
    /// <summary>
    /// Управляет текущим состоянием игры и переключением между ними.
    /// </summary>
    public sealed class StateMachine : MonoBehaviour
    {
        public GameState CurrentState { get; private set; } = new Boot();

        public delegate void GameStateChanged(GameState previous, GameState current);
        public event GameStateChanged OnStateChanged;

        public void SwitchState<T>() where T : GameState, new()
        {
            var newState = new T();

            if (newState.GetType() == CurrentState?.GetType())
            {
                Debug.Log(
                    $"[GameState] Transition skipped: already in {newState.GetType().Name}. " +
                    $"Previous: {CurrentState?.GetType().Name ?? "None"}"
                );
                return;
            }

            var previous = CurrentState;
            CurrentState = newState;
            CurrentState.Enter();

            Debug.Log($"GameState: {previous?.GetType().Name ?? "None"} → {newState.GetType().Name}");
            OnStateChanged?.Invoke(previous, newState);
        }

    }
}
