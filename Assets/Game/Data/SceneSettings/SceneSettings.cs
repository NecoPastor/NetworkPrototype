using UnityEngine;

public abstract class SceneSettings : ScriptableObject
{
    public abstract string SceneName { get; }

    [Header("Network Settings")]
    [Tooltip("Load this scene via FishNet network scene manager")]
    [SerializeField] private bool _isNetworked;
    public bool IsNetworked => _isNetworked;
}