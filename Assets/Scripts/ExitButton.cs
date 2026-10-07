using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    [SerializeField] private Button exitButton;

    private void Start()
    {
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("[ExitButton] Quitting application...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitButtonClicked);
    }
}