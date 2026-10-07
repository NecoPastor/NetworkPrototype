README: Настройка и использование ProfileService
Универсальный, полностью автономный модуль профиля игрока (ProfileService) с поддержкой модульной системы данных (IProfileModule) и инверсией зависимости от провайдера сохранения (IProfileStorageProvider).

Модуль расположен в пространстве имен Game.Systems.Persistence и не имеет внешних зависимостей.

Архитектура модуля
Система состоит из 4 ключевых элементов:

PlayerProfileData — контейнер компонентов/модулей данных игрока.

IProfileModule — интерфейс для создания конкретных модулей данных (экономика, инвентарь, прогресс).

IProfileStorageProvider — абстрактный контракт сохранения/загрузки данных.

ProfileService — глобальный сервис управления жизненным циклом профиля.

Game.Systems.Persistence
│
├── IProfileModule.cs            # Маркерный интерфейс модулей данных
├── PlayerProfileData.cs         # Контейнер модулей профиля
├── IProfileStorageProvider.cs   # Абстракция хранилища (JSON, Cloud, PlayerPrefs)
└── ProfileService.cs            # Сервис управления профилем

Шаг 1: Создание модулей данных для проекта
Для добавления игровых данных в профиль создайте класс, реализующий IProfileModule:

C# 
using System;
using Game.Systems.Persistence;

namespace Game.Gameplay.Persistence
{
    /// <summary>
    /// Модуль экономики конкретной игры.
    /// </summary>
    [Serializable]
    public class EconomyModule : IProfileModule
    {
        public int Credits = 1000;
        public int Crystals = 50;
    }

    /// <summary>
    /// Модуль прогресса отряда.
    /// </summary>
    [Serializable]
    public class SquadModule : IProfileModule
    {
        public int MaxSquadSize = 4;
        public int AccountLevel = 1;
    }
}

Шаг 2: Реализация IProfileStorageProvider
В конкретном проекте создайте провайдер, отвечающий за физическую запись и чтение файла.

Пример реализации провайдера сохранения в JSON-файл:

using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Game.Systems.Persistence;

namespace Game.Infrastructure.Persistence
{
    /// <summary>
    /// Локальный провайдер сохранения профиля в JSON.
    /// </summary>
    public class LocalJsonStorageProvider : IProfileStorageProvider
    {
        private readonly string _filePath = Path.Combine(Application.persistentDataPath, "profile.json");

        public Task<PlayerProfileData> LoadAsync()
        {
            if (!File.Exists(_filePath))
                return Task.FromResult<PlayerProfileData>(null);

            string json = File.ReadAllText(_filePath);
            var profile = JsonUtility.FromJson<PlayerProfileData>(json);
            return Task.FromResult(profile);
        }

        public Task SaveAsync(PlayerProfileData profile)
        {
            string json = JsonUtility.ToJson(profile, true);
            File.WriteAllText(_filePath, json);
            return Task.CompletedTask;
        }
    }
}

Шаг 3: Инициализация при старте приложения
Инициализируйте ProfileService в точке входа игры (например, в GameBootstrapper или через DI-контейнер):

using UnityEngine;
using Game.Systems.Persistence;
using Game.Infrastructure.Persistence;
using Game.Gameplay.Persistence;

public class GameBootstrapper : MonoBehaviour
{
    public static ProfileService ProfileService { get; private set; }

    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // 1. Создаем провайдер и сервис
        var storageProvider = new LocalJsonStorageProvider();
        ProfileService = new ProfileService(storageProvider);

        // 2. Загружаем профиль
        await ProfileService.LoadProfileAsync();

        // 3. Гарантируем наличие необходимых модулей
        EnsureModulesInitialized();

        Debug.Log("[Bootstrapper] Профиль успешно загружен!");
    }

    private void EnsureModulesInitialized()
    {
        var profile = ProfileService.Profile;

        if (!profile.HasModule<EconomyModule>())
            profile.AddModule(new EconomyModule());

        if (!profile.HasModule<SquadModule>())
            profile.AddModule(new SquadModule());
    }
}

Шаг 4: Использование в геймплее
Чтение и изменение данных:

// Получение модуля экономики из сервиса
var economy = ProfileService.Profile.GetModule<EconomyModule>();

if (economy != null && economy.Credits >= 100)
{
    economy.Credits -= 100;
    
    // Асинхронное сохранение изменений
    _ = ProfileService.SaveProfileAsync();
}

Начисление наград по завершении боя (CombatEndState):

public void AwardMissionRewards(int creditsEarned)
{
    var economy = ProfileService.Profile.GetModule<EconomyModule>();
    if (economy != null)
    {
        economy.Credits += creditsEarned;
        _ = ProfileService.SaveProfileAsync();
    }
}

