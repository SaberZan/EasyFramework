using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HybridCLR;
using Match3;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Easy
{
    public class HotFixLaunch : MonoBehaviour
    {
        public string sceneName;

        private Launch launch;
        // Start is called before the first frame update
        void Start()
        {            
            launch = GameObject.FindAnyObjectByType<Launch>();

            if(EasyFrameworkHotFix.Instance.IsInited())
            {
                EasyFrameworkHotFix.Instance.Dispose();
            }

            EasyFrameworkHotFix.Instance.AddSingletonBatchInitCallback(RefreshMainProgress);

            EasyFrameworkHotFix.Instance.Init((result) =>
            {
                EasyFrameworkHotFix.Instance.SubSingletonBatchInitCallback(RefreshMainProgress);
                if (result)
                {
                    SceneManager.sceneLoaded += OnSceneLoaded;
                    SceneManager.LoadScene(sceneName);
                }
                else
                {
                    Application.Quit();
                }
            });
        }

        private void RefreshMainProgress(bool result)
        {
            launch.targetProgress = 0.5f + EasyFrameworkHotFix.Instance.initProgress / 2;
            launch.launchText.text = string.Join(",", EasyFrameworkHotFix.Instance.initializingSingles);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
        {
            if (scene.name == sceneName)
            {
                GameObject testObject = new GameObject();
                testObject.AddComponent<TestScene>();
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        public void OnApplicationQuit()
        {
            if(EasyFrameworkHotFix.Instance.IsInited())
            {
                EasyFrameworkHotFix.Instance.Dispose();
            }
        }
    }

}
