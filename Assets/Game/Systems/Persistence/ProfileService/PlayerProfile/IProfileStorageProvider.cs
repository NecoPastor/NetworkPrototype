using System.Threading.Tasks;

namespace Game.Systems.Persistence
{
    /// <summary>
    /// Абстрактный контракт хранилища данных профиля.
    /// Реализуется в конкретном проекте для сохранения/загрузки (файлы, PlayerPrefs, Cloud и т.д.).
    /// </summary>
    public interface IProfileStorageProvider
    {
        /// <summary>
        /// Асинхронная загрузка данных профиля из источника.
        /// </summary>
        Task<PlayerProfileData> LoadAsync();

        /// <summary>
        /// Асинхронное сохранение данных профиля в источник.
        /// </summary>
        Task SaveAsync(PlayerProfileData profile);
    }
}