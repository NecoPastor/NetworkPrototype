using System;

namespace Game.Systems.Persistence
{
    /// <summary>
    /// Базовый класс для всех профилей данных в игре (юниты, игрок, базы и т.д.).
    /// Наследует модульный контейнер и содержит уникальный идентификатор сущности.
    /// </summary>
    [Serializable]
    public abstract class ProfileData : ModuleContainer<IDataModule>
    {
        public string InstanceId;
    }
}