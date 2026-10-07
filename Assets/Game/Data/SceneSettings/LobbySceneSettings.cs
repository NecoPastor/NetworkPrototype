using UnityEngine;

[CreateAssetMenu()]
public class LobbySceneSettings : SceneSettings
{
    [SerializeField] private string sceneName;
    public override string SceneName => sceneName;
}

