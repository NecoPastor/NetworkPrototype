using Game.Systems.Service;
using UnityEngine;

namespace Game.Systems
{
    public class SceneTransitionResponderProvider : GameSystemProvider, IServiceProvider<SceneTransitionResponder>
    {
        [SerializeField] private SceneTransitionResponder sceneTransitionResponder;

        public SceneTransitionResponder Get() => sceneTransitionResponder;
    }
}