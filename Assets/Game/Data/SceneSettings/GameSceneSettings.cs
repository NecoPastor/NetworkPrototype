using UnityEngine;

[CreateAssetMenu()]
public class GameSceneSettings : SceneSettings
{
    [SerializeField] private string sceneName;
    public override string SceneName => sceneName;
}
