using UnityEngine;

namespace Game.Systems.Service
{
    public abstract class GameSystemProvider : MonoBehaviour, IGameSystem
    {
        public virtual void Initialize() => RegistryService();

        public virtual void RegistryService()
        {
            ServiceLocator.RegisterService(this.GetType(), this);
            DontDestroyOnLoad(gameObject);
        }

        public virtual void RemoveService() => ServiceLocator.RemoveService(this);

        public void Shutdown() => RemoveService();

        private void OnDestroy() => Shutdown();


    }
}
