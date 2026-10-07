using Game.Systems.GameStateMachine;
using Game.Systems.SceneManagement;
using Game.Systems.Service;
using UnityEngine;

public class SceneTransitionResponder : MonoBehaviour, IGameSystem
{
    private StateMachine stateMachine;
    private SceneLoader sceneLoader;

    public void Initialize()
    {
        if (ServiceLocator.TryGetService(out StateMachine stateMachine))
            this.stateMachine = stateMachine;

        if (ServiceLocator.TryGetService(out SceneLoader sceneLoader))
            this.sceneLoader = sceneLoader;

        if (stateMachine)
            stateMachine.OnStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState previous, GameState next)
    {
        if (next is MainMenu)
        {
            if (sceneLoader)
            {
                var settings = SceneReference.Instance.GetByType<MainMenuSceneSettings>();
                sceneLoader.Load(settings.SceneName);
            }
        }

        if (next is Lobby)
        {
            if (sceneLoader)
            {
                var settings = SceneReference.Instance.GetByType<LobbySceneSettings>();
                sceneLoader.Load(settings.SceneName);
            }
        }

        if (next is Playing)
        {
            if (sceneLoader)
            {
                var settings = SceneReference.Instance.GetByType<GameSceneSettings>();
                sceneLoader.Load(settings.SceneName);
            }
        }
    }

    public void Shutdown() { }

    private void OnDestroy()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= OnGameStateChanged;
    }


}
