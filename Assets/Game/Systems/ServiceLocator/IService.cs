namespace Game.Systems.Service
{
    /// <summary>
    /// Интерфейс для сервисов, которые хотят реагировать на регистрацию и удаление.
    /// </summary>
    public interface IService
    {
        void RegistryService();
        void RemoveService();
    }
}