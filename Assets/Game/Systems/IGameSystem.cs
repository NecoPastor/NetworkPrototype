public interface IGameSystem
{
    /// <summary>
    /// Инициализация системы. Вызывается один раз при запуске.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Завершение работы системы. Вызывается при выходе из игры или смене сцены.
    /// </summary>
    void Shutdown();
}
