using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assemblies;

namespace Game.Systems.GameStateMachine
{
    public sealed class StateMachine : MonoBehaviour
    {
        public GameState CurrentState { get; private set; }

        public delegate void GameStateChanged(GameState previous, GameState current);
        public event GameStateChanged OnStateChanged;

        // 1. Ваш исходный дженерик-метод
        public void SwitchState<T>() where T : GameState, new()
        {
            SwitchState(typeof(T));
        }

        // 2. Перегрузка, принимающая string
        public void SwitchState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                Debug.LogError("[StateMachine] Имя состояния пустое!");
                return;
            }

            // Ищем тип по имени среди всех классов-наследников GameState
            //Type stateType = AppDomain.CurrentDomain.GetAssemblies()
            Type stateType = CurrentAssemblies.GetLoadedAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == stateName && typeof(GameState).IsAssignableFrom(t) && !t.IsAbstract);

            if (stateType == null)
            {
                Debug.LogError($"[StateMachine] Не найден класс GameState с именем '{stateName}'!");
                return;
            }

            SwitchState(stateType);
        }

        // 3. Основная логика переключения по System.Type
        public void SwitchState(Type stateType)
        {
            if (stateType == null || !typeof(GameState).IsAssignableFrom(stateType))
            {
                Debug.LogError($"[StateMachine] Невалидный тип состояния: {stateType?.Name}");
                return;
            }

            // Проверка на повторный переход
            if (CurrentState?.GetType() == stateType)
            {
                Debug.Log(
                    $"[GameState] Transition skipped: already in {stateType.Name}. " +
                    $"Previous: {CurrentState?.GetType().Name ?? "None"}"
                );
                return;
            }

            // Создаем экземпляр состояния
            var newState = (GameState)Activator.CreateInstance(stateType);
            var previous = CurrentState;
            CurrentState = newState;
            CurrentState.Enter();

            Debug.Log($"GameState: {previous?.GetType().Name ?? "None"} → {newState.GetType().Name}");
            OnStateChanged?.Invoke(previous, newState);
        }
    }
}