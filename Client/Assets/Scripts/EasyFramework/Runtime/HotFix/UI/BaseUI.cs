using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Easy
{
    [Flags]
    public enum UIState
    {
        None = 0,
        Awake = 1 << 1,
        Start = 1 << 2,
        Show = 1 << 3,
        Destroy = 1 << 4,
        Destroyed = 1 << 5,
    }

    public class BaseUI
    {

        public UIState uiState = UIState.None;

        public ISingleUnityAssetHandle<GameObject> handle;

        public GameObject gameObject;

        public List<BaseUI> subUIs = new List<BaseUI>();

        public List<BaseUI> tmpSubUIs = new List<BaseUI>();

        public List<Coroutine> coroutines = new List<Coroutine>();

        public List<IBaseAssetHandle> handles = new List<IBaseAssetHandle>();

        private int showCount;

        private int hideCount;

        public CancellationTokenSource cancellationTokenSource;

        public virtual string GetUIName()
        {
            return GetType().Name;
        }

        public virtual void Awake()
        {
            cancellationTokenSource = new CancellationTokenSource();
            EventMgr.Instance.SubscribeByTarget(this);
            subUIs.ForEach(subUI=> { if(!subUI.GetUIState(UIState.Awake)) subUI.Awake(); });
            AddUIState(UIState.Awake);
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

            AddUIState(UIState.Start);

            subUIs.ForEach(subUI=>
            { 
                if(subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Start))
                { 
                    subUI.Start(); 
                }
            });
            
        }

        public virtual void Show(Action callback = null)
        {
            if(GetUIState(UIState.Show))
            {
                callback?.Invoke();
                return;
            }
            AddUIState(UIState.Show);
            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            showCount = tmpSubUIs.Count;
            tmpSubUIs.ForEach(subUI=> 
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
            if(!GetUIState(UIState.Show))
            {
                callback?.Invoke(); 
                return;
            }
            SubUIState(UIState.Show);
            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            hideCount = tmpSubUIs.Count;
            tmpSubUIs.ForEach(subUI=> 
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


        public virtual void Update(float deltaTime)
        {
            if(GetUIState(UIState.Start))
            {
                tmpSubUIs.Clear();
                tmpSubUIs.AddRange(subUIs);
                tmpSubUIs.ForEach(subUI=> 
                { 
                    subUI.Update(deltaTime);
                });
            }
        }

        public virtual void Destroy()
        {
            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            tmpSubUIs.ForEach(subUI=>
            { 
                if(subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Destroy))
                { 
                    subUI.Destroy(); 
                }
            });
            foreach (var routine in coroutines)
            {
                CoroutineMgr.Instance.StopCoroutine(routine);
            }
            coroutines.Clear();

            cancellationTokenSource.Cancel();
            cancellationTokenSource = null;

            foreach (var handle in handles)
            {
                AssetsMgr.Instance.Release(handle);
            }
            handles.Clear();

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
            AddUIState(UIState.Destroy);
        }

        public void Destroyed()
        {
            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            tmpSubUIs.ForEach(subUI=>
            { 
                if(subUI.GetUIState(UIState.Destroy) && !subUI.GetUIState(UIState.Destroyed))
                { 
                    subUI.Destroyed(); 
                }
            });
            subUIs.Clear();
            EventMgr.Instance.UnSubscribeByTarget(this);
            AddUIState(UIState.Destroyed);
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
            if(GetUIState(UIState.Awake))
            {
                if(!subUI.GetUIState(UIState.Awake))
                {
                    subUI.Awake();
                }
            }

            if(GetUIState(UIState.Start))
            {
                if(subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Start))
                {
                    subUI.Start();
                }
            }

            if(GetUIState(UIState.Show))
            {
                subUI.Show();
            }
            else
            {
                subUI.Hide();
            }

            if(GetUIState(UIState.Destroy))
            {
                if(subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Destroy))
                {
                    subUI.Destroy();
                }
            }

            if(GetUIState(UIState.Destroyed))
            {
                if(subUI.GetUIState(UIState.Destroy) && !subUI.GetUIState(UIState.Destroyed))
                {
                    subUI.Destroyed();
                }
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

        public Coroutine StartCoroutine(IEnumerator item)
        {
           var routine = CoroutineMgr.Instance.StartCoroutine(item);
           coroutines.Add(routine);
           return routine;
        }

        public void StopCoroutine(Coroutine routine)
        {
           CoroutineMgr.Instance.StopCoroutine(routine);
           coroutines.Remove(routine);
        }

        public IBaseAssetHandle LoadAsset(string path)
        {
            IBaseAssetHandle handle = AssetsMgr.Instance.LoadAsset<UnityEngine.Object>(path);
            handles.Add(handle);
            return handle;
        }

        public IBaseAssetHandle LoadAssets(List<string> paths)
        {
            IBaseAssetHandle handle = AssetsMgr.Instance.LoadAssetsByPath<UnityEngine.Object>(paths);
            handles.Add(handle);
            return handle;
        }

        public IBaseAssetHandle LoadRawAsset(string path)
        {
            IBaseAssetHandle handle = AssetsMgr.Instance.LoadRawAsset(path);
            handles.Add(handle);
            return handle;
        }

        public IBaseAssetHandle LoadRawAssets(List<string> paths)
        {
            IBaseAssetHandle handle = AssetsMgr.Instance.LoadRawAssetsByPath(paths);
            handles.Add(handle);
            return handle;
        }

    }
}