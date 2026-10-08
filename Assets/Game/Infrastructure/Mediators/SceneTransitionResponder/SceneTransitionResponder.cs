using Game.Systems.GameStateMachine;
using Game.Systems.SceneManagement;
using Game.Systems.Service;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionResponder : MonoBehaviour, IGameSystem
{
    [Header("Настройки сцен для Геймдизайнера")]
    [SerializeField] private List<StateSceneMapping> _sceneMappings = new List<StateSceneMapping>();

    private StateMachine _stateMachine;
    private SceneLoader _sceneLoader;

    public void Initialize()
    {
        ServiceLocator.TryGetService(out _stateMachine);
        ServiceLocator.TryGetService(out _sceneLoader);

        if (_stateMachine != null)
        {
            _stateMachine.OnStateChanged += OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(GameState previous, GameState next)
    {
        if (_sceneLoader == null || next == null) return;

        string nextStateTypeName = next.GetType().Name;

        // Ищем соответствие по имени типа C#-класса состояния
        foreach (var mapping in _sceneMappings)
        {
            if (mapping.GameStateName == nextStateTypeName && mapping.SceneSettings != null)
            {
                string sceneName = mapping.SceneSettings.SceneName;

                if (mapping.SceneSettings.IsNetworked)
                {
                    _sceneLoader.LoadNetworkScene(sceneName);
                }
                else
                {
                    _sceneLoader.Load(sceneName);
                }
                break;
            }
        }
    }

    public void Shutdown()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (_stateMachine != null)
        {
            _stateMachine.OnStateChanged -= OnGameStateChanged;
            _stateMachine = null;
        }
    }

    [Serializable]
    public struct StateSceneMapping
    {
        [Tooltip("Название класса состояния (например, MainMenu, Lobby, Playing)")]
        [GameState]
        public string GameStateName;

        [Tooltip("Ссылка на настройки соответствующей сцены")]
        public SceneSettings SceneSettings;
    }
}