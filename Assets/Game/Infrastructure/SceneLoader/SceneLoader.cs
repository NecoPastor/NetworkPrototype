using FishNet;
using FishNet.Managing.Scened;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Systems.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        public void Load(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        public void LoadAsync(string sceneName, Action onComplete = null)
        {
            StartCoroutine(LoadSceneAsync(sceneName, onComplete));
        }

        public void LoadNetworkScene(string sceneName)
        {
            Debug.Log($"[SceneLoader] LoadNetworkScene called for: {sceneName}. IsServer: {InstanceFinder.IsServer}");

            if (InstanceFinder.IsServer)
            {
                if (InstanceFinder.SceneManager == null)
                {
                    Debug.LogError("[SceneLoader] InstanceFinder.SceneManager is NULL!");
                    return;
                }

                SceneLoadData sld = new SceneLoadData(sceneName);
                sld.ReplaceScenes = ReplaceOption.All;
                InstanceFinder.SceneManager.LoadGlobalScenes(sld);
                Debug.Log($"[SceneLoader] LoadGlobalScenes requested for: {sceneName}");
            }
            else
            {
                Debug.LogWarning("[SceneLoader] Only the server can load network scenes.");
            }
        }

        private IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
        {
            var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
                yield return null;

            onComplete?.Invoke();
        }
    }
}