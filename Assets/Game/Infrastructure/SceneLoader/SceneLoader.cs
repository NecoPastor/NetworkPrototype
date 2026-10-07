using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Systems.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void LoadAsync(string sceneName, Action onComplete = null)
        {
            StartCoroutine(LoadSceneAsync(sceneName, onComplete));
        }

        private IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
                yield return null;

            onComplete?.Invoke();
        }
    }
}