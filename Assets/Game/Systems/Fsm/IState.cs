namespace Game.Systems.Fsm
{
    /// <summary>
    /// Определяет базовый жизненный цикл состояния в машине состояний.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Вызывается единоразово при входе в данное состояние.
        /// Используется для инициализации, подписки на события и подготовки UI.
        /// </summary>
        void Enter();

        /// <summary>
        /// Вызывается единоразово при выходе из данного состояния.
        /// Используется для отписки от событий, очистки временных данных и сброса флагов.
        /// </summary>
        void Exit();

        /// <summary>
        /// Вызывается каждый кадр/шаг игрового цикла, пока состояние является активным.
        /// </summary>
        void Update();
    }

    /// <summary>
    /// Расширяет базовое состояние, позволяя передавать контекст данных при входе.
    /// </summary>
    /// <typeparam name="TContext">Тип контекста, содержащего данные для работы состояния.</typeparam>
    public interface IState<in TContext> : IState
    {
        /// <summary>
        /// Вызывается при входе в состояние с передачей контекста данных.
        /// </summary>
        /// <param name="context">Контекст с данными (например, CombatContext или GameContext).</param>
        void Enter(TContext context);
    }
}