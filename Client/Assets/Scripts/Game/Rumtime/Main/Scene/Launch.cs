using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HybridCLR;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Easy
{
    public class Launch : MonoBehaviour
    {
        [NonSerialized]
        public Text launchText;

        private Text nowProgressText;
        private Slider slider;
        [NonSerialized]
        public float targetProgress = 0;
        [NonSerialized]
        public float nowProgress = 0;


        private Text nowAssetsProgressText;
        private Slider assetsSlider;
        [NonSerialized]
        public float targetAssetsProgress = 0;
        [NonSerialized]
        public float nowAssetsProgress = 0;

        private bool destory = false;

        private ISingleUnityAssetHandle<GameObject> handle;
        private GameObject launchGameObject;
        
        // Start is called before the first frame update
        void Start()
        {
            //DontDestroyOnLoad(this);

            launchText = GameObject.Find("LaunchCanvas/launchText").GetComponent<Text>();

            slider = GameObject.Find("LaunchCanvas/Slider").GetComponent<Slider>();
            nowProgressText = GameObject.Find("LaunchCanvas/Slider/progressText").GetComponent<Text>();

            assetsSlider = GameObject.Find("LaunchCanvas/AssetsSlider").GetComponent<Slider>();
            nowAssetsProgressText = GameObject.Find("LaunchCanvas/AssetsSlider/progressText").GetComponent<Text>();


            if (EasyFrameworkMain.Instance.IsInited())
            {
                EasyFrameworkMain.Instance.Dispose();
            }

            EasyFrameworkMain.Instance.AddSingletonBatchInitCallback(RefreshMainProgress);

            EasyFrameworkMain.Instance.Init((result) =>
            {
                EasyFrameworkMain.Instance.SubSingletonBatchInitCallback(RefreshMainProgress);

                if(result)
                {
                    handle = AssetsMgr.Instance.LoadAsset<GameObject>("Assets/GameResources/HotFix/HotFixLaunch.prefab");
                    launchGameObject = handle.Instantiate();
                }
                else
                {
                    Application.Quit();
                }

            });
        }

        private void RefreshMainProgress(bool result)
        {
            targetProgress = EasyFrameworkMain.Instance.initProgress / 2;
            launchText.text = string.Join(",", EasyFrameworkMain.Instance.initializingSingles);
        }
        
        // Update is called once per frame
        void Update()
        {
            UpdateAssetsProgress();
            UpdateMainProgress();
        }

        public void UpdateAssetsProgress()
        {
            //资源进度
            AssetsMgrState state = AssetsMgr.Instance.GetState();
            if (state == AssetsMgrState.Success)
            {
                if (AssetsMgr.Instance.NeedForeUpdate())
                {
                    //TODO 强更新弹窗
                }
                targetAssetsProgress = 1;
            }
            else if (state == AssetsMgrState.InProgress)
            {
                targetAssetsProgress = AssetsMgr.Instance.GetUpdateProgress();
            }
            else if (state == AssetsMgrState.None)
            {
                nowAssetsProgress = 0;
                targetAssetsProgress = 0;
            }
            if (targetAssetsProgress < 1.0f)
            {
                nowAssetsProgress += (targetAssetsProgress - nowAssetsProgress) / 10;
                assetsSlider.value = nowAssetsProgress;
            }
            else
            {
                assetsSlider.value = 1;
            }
            nowAssetsProgressText.text = "" + nowAssetsProgress;
        }

        public void UpdateMainProgress()
        {
            //总进度
            if (targetProgress < 1.0f)
            {
                nowProgress += (targetProgress - nowProgress) / 10;
                slider.value = nowProgress;
            }
            else
            {
                slider.value = 1;
            }
            nowProgressText.text = "" + nowAssetsProgress;
        }

        private void OnDestroy()
        {
            if (launchGameObject != null && handle != null)
            {
                handle.ReleaseInstance(launchGameObject);
                AssetsMgr.Instance.Release(handle);
            }
        }

        public void OnApplicationQuit()
        {
            if(EasyFrameworkMain.Instance.IsInited())
            {
                EasyFrameworkMain.Instance.Dispose();
            }
        }
    }

}
