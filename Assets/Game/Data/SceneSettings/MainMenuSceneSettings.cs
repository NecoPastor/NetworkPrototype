using UnityEngine;

[CreateAssetMenu()]
public class MainMenuSceneSettings : SceneSettings
{
    [SerializeField] private string sceneName;
    public override string SceneName => sceneName;
}

