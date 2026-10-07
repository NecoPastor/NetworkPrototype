using Game.Systems.Persistence;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Infrastructure.Persistence
{
    /// <summary>
    /// Простейшая реализация сохранения в локальный JSON-файл для конкретного проекта.
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