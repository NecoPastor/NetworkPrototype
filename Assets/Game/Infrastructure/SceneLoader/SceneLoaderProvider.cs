using Game.Systems.SceneManagement;
using Game.Systems.Service;
using UnityEngine;

namespace Game.Systems.WeaponSystem
{
    public class SceneLoaderProvider : GameSystemProvider, IServiceProvider<SceneLoader>
    {
        [SerializeField] private SceneLoader sceneLoader;

        public SceneLoader Get() => sceneLoader;
    }
}
