using System;
using System.Collections.Generic;

namespace Game.Systems.Fsm
{
    /// <summary>
    /// Универсальная базовая реализация машины состояний.
    /// Управляет жизненным циклом зарегистрированных состояний и переключением между ними.
    /// </summary>
    public class StateMachine : IStateMachine
    {
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

        /// <summary>
        /// Текущее активное состояние машины состояний.
        /// </summary>
        public IState CurrentState { get; private set; }

        /// <summary>
        /// Регистрирует объект состояния в машине состояний.
        /// </summary>
        /// <typeparam name="TState">Тип состояния.</typeparam>
        /// <param name="state">Экземпляр состояния.</param>
        public void RegisterState<TState>(TState state) where TState : class, IState
        {
            Type type = typeof(TState);

            if (_states.ContainsKey(type))
            {
                _states[type] = state;
            }
            else
            {
                _states.Add(type, state);
            }
        }

        /// <summary>
        /// Выполняет переход на состояние указанного типа без передачи контекста.
        /// </summary>
        /// <typeparam name="TState">Тип состояния, зарегистрированного в машине.</typeparam>
        public void ChangeState<TState>() where TState : class, IState
        {
            Type type = typeof(TState);

            if (!_states.TryGetValue(type, out IState nextState))
            {
                throw new InvalidOperationException($"Состояние типа {type.Name} не зарегистрировано в StateMachine.");
            }

            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
        }

        /// <summary>
        /// Выполняет переход на состояние указанного типа с передачей контекста данных.
        /// </summary>
        /// <typeparam name="TState">Тип состояния с контекстом.</typeparam>
        /// <typeparam name="TContext">Тип передаваемого контекста.</typeparam>
        /// <param name="context">Контекст данных для инициализации состояния.</param>
        public void ChangeState<TState, TContext>(TContext context) where TState : class, IState<TContext>
        {
            Type type = typeof(TState);

            if (!_states.TryGetValue(type, out IState nextState))
            {
                throw new InvalidOperationException($"Состояние типа {type.Name} не зарегистрировано в StateMachine.");
            }

            var contextualState = (IState<TContext>)nextState;

            CurrentState?.Exit();
            CurrentState = contextualState;
            contextualState.Enter(context);
        }

        /// <summary>
        /// Вызывает обновление текущего активного состояния.
        /// </summary>
        public void Update()
        {
            CurrentState?.Update();
        }
    }
}