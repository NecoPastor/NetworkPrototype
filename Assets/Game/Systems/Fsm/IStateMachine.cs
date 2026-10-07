namespace Game.Systems.Fsm
{
    /// <summary>
    /// Управляет текущим состоянием, переключением между состояниями и их обновлением.
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// Текущее активное состояние машины состояний.
        /// </summary>
        IState CurrentState { get; }

        /// <summary>
        /// Выполняет переход на новое состояние указанного типа.
        /// </summary>
        /// <typeparam name="TState">Тип состояния, на которое необходимо переключиться.</typeparam>
        void ChangeState<TState>() where TState : class, IState;

        /// <summary>
        /// Обновляет текущее активное состояние.
        /// Должен вызываться из главного игрового цикла (Update/Tick).
        /// </summary>
        void Update();
    }
}