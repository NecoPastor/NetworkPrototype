using UnityEngine;

[CreateAssetMenu(menuName = "Scene/Reference")]
public class SceneReference : ScriptableObject
{
    public SceneSettings[] allScenes;

    private static SceneReference _instance;

    public static SceneReference Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<SceneReference>("SceneSettings/SceneReference");
                if (_instance == null)
                    Debug.LogError("[SceneReference] Файл 'SceneReference.asset' не найден в Resources");
            }

            return _instance;
        }
    }

    public SceneSettings GetByType<T>() where T : SceneSettings
    {
        foreach (var settings in allScenes)
        {
            if (settings is T typed)
                return typed;
        }

        Debug.LogWarning($"[SceneReference] Не найдены настройки для {typeof(T).Name}");
        return null;
    }
}

