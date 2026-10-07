namespace Game.Systems.Persistence
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Обобщенный модульный контейнер сохраняемых данных.
    /// </summary>
    [Serializable]
    public abstract class ModuleContainer<TModuleBase> where TModuleBase : class, IDataModule
    {
        private readonly Dictionary<Type, TModuleBase> _modules = new Dictionary<Type, TModuleBase>();

        public void AddModule<T>(T module) where T : class, TModuleBase
        {
            _modules[typeof(T)] = module;
        }

        public T GetModule<T>() where T : class, TModuleBase
        {
            return _modules.TryGetValue(typeof(T), out var module) ? module as T : null;
        }

        public bool HasModule<T>() where T : class, TModuleBase
            => _modules.ContainsKey(typeof(T));
    }
}