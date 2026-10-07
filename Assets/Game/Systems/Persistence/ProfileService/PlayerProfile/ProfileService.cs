using System.Threading.Tasks;

namespace Game.Systems.Persistence
{
    /// <summary>
    /// Универсальный сервис управления жизненным циклом профиля игрока.
    /// </summary>
    public class ProfileService
    {
        private readonly IProfileStorageProvider _storageProvider;

        /// <summary>
        /// Текущие данные профиля игрока в памяти.
        /// </summary>
        public PlayerProfileData Profile { get; private set; }

        /// <summary>
        /// Создает экземпляр ProfileService с опциональным провайдером хранилища.
        /// </summary>
        /// <param name="storageProvider">Провайдер сохранения/загрузки конкретного проекта.</param>
        public ProfileService(IProfileStorageProvider storageProvider = null)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Загружает профиль из провайдера хранилища или создает новый, если провайдер не задан или вернул null.
        /// </summary>
        public async Task LoadProfileAsync()
        {
            if (_storageProvider != null)
            {
                Profile = await _storageProvider.LoadAsync();
            }

            // Если данные не найдены или провайдер не передан — инициализируем чистый профиль
            Profile ??= new PlayerProfileData();
        }

        /// <summary>
        /// Сохраняет текущее состояние профиля через провайдер хранилища.
        /// </summary>
        public async Task SaveProfileAsync()
        {
            if (Profile == null || _storageProvider == null) return;

            await _storageProvider.SaveAsync(Profile);
        }
    }
}