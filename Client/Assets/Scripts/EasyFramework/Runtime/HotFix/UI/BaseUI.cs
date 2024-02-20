using System;
using System.Collections.Generic;
using UnityEngine;

namespace Easy
{
    [Flags]
    public enum UIState
    {
        None = 0,
        Awake = 1 << 1,
        Start = 1 << 2,
        Destroy = 1 << 3,
        Destroyed = 1 << 4,
    }

    public class BaseUI
    {

        public UIState uiState = UIState.None;

        public bool isShow;

        public ISingleUnityAssetHandle<GameObject> handle;

        public GameObject gameObject;

        public List<BaseUI> subUIs = new List<BaseUI>(); 

        private int showCount;

        private int hideCount;

        public void Awake()
        {
            EventMgr.Instance.SubscribeByTarget(this);
            subUIs.ForEach(subUI=> { if(subUI.uiState == UIState.None) subUI.Awake(); });
            uiState = UIState.Awake;
        }

        public virtual string GetUIName()
        {
            return GetType().Name;
        }

        public virtual void Start()
        {
            if(handle != null)
            {
                gameObject = handle.Instantiate();
            }
            
            if(gameObject == null)
            {
                gameObject = new GameObject(GetType().Name);
            }

            subUIs.ForEach(subUI=> { if(subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Start)) subUI.Start(); });
            uiState = UIState.Start;
        }

        public virtual void Show(Action callback = null)
        {
            if(isShow)
            {
                return;
            }
            isShow = true;
            showCount = subUIs.Count;
            subUIs.ForEach(subUI=> 
            { 
                if(subUI.GetUIState(UIState.Start))
                { 
                    subUI.Show(()=>
                    { 
                        --showCount; 
                        if(showCount == 0)
                        { 
                            gameObject.SetActive(true); 
                            callback?.Invoke();
                        }
                    }); 
                }
            });
            
        }

        public virtual void Hide(Action callback = null)
        {
            if(!isShow)
            {
                return;
            }
            isShow = false;
            hideCount = subUIs.Count;
            subUIs.ForEach(subUI=> 
            { 
                if(subUI.GetUIState(UIState.Start))
                { 
                    subUI.Hide(()=>
                    { 
                        --hideCount; 
                        if(hideCount == 0)
                        { 
                            gameObject.SetActive(false); 
                            callback?.Invoke(); 
                        }
                    }); 
                }
            });
        }

        public virtual void Update()
        {
            subUIs.ForEach(subUI=> { if(subUI.GetUIState(UIState.Start)) subUI.Update(); });
        }

        public virtual void Destroy()
        {
            subUIs.ForEach(subUI=>{ if(subUI.GetUIState(UIState.Awake)) subUI.Destroy(); });
            if (handle != null)
            {
                if (gameObject != null)
                {
                    handle.ReleaseInstance(gameObject);
                    gameObject = null;
                }
                AssetsMgr.Instance.Release(handle);
            }
            else
            {
                if (gameObject != null)
                {
                    GameObject.Destroy(gameObject);
                    gameObject = null;
                }
            }

            uiState = UIState.Destroy;
        }

        public void Destroyed()
        {
            subUIs.ForEach(subUI=>{ if(subUI.GetUIState(UIState.Destroy)) subUI.Destroyed(); });
            subUIs.Clear();
            EventMgr.Instance.UnSubscribeByTarget(this);
            uiState = UIState.Destroyed;
        }

        public void SetPrefabPath(string path)
        {
            if(gameObject != null)
            {
                throw new System.Exception("UI错误");
            }
            handle = AssetsMgr.Instance.LoadAsset<GameObject>(path);
        }

        public void SetGameObject(GameObject gObj)
        {
            if(handle != null)
            {
                throw new System.Exception("UI错误");
            }
            gameObject = gObj;
        }

        public void AddSubUI(BaseUI subUI)
        {
            subUIs.Add(subUI);
            if(!subUI.GetUIState(UIState.Awake))
            {
                subUI.Awake();
            }
            if(subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Start))
            {
                subUI.Start();
            }
            if(isShow)
            {
                subUI.Show();
            }
            else
            {
                subUI.Hide();
            }
        }

        public void RemoveUIWidget(BaseUI subUI)
        {
            if(subUIs.Contains(subUI))
            {
                if(subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Destroy))
                {
                    subUI.Destroy();
                }
                if(subUI.GetUIState(UIState.Destroy) && !subUI.GetUIState(UIState.Destroyed))
                {
                    subUI.Destroyed();
                }
                subUIs.Remove(subUI);
            }
        }

        public void AddUIState(UIState state)
        {
            if ((uiState & state) == UIState.None)
            {
                uiState ^= state;
            }
        }

        public void SubUIState(UIState state)
        {
            if ((uiState & state) != UIState.None)
            {
                uiState ^= state;
            }
        }

        public bool GetUIState(UIState state)
        {
            return (uiState & state) != UIState.None;
        }

    }
}