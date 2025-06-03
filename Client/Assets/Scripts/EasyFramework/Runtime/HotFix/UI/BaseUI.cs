using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Easy
{
    /// <summary>
    /// UI状态枚举，用于标记UI组件的生命周期状态
    /// </summary>
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

    /// <summary>
    /// 基础UI类，提供UI组件的基本功能和生命周期管理
    /// </summary>
    public class BaseUI
    {
        /// <summary>
        /// 当前UI的状态
        /// </summary>
        public UIState uiState = UIState.None;

        /// <summary>
        /// 资源句柄，用于加载和释放资源
        /// </summary>
        public ISingleUnityAssetHandle<GameObject> handle;

        /// <summary>
        /// 关联的GameObject实例
        /// </summary>
        public GameObject gameObject;

        /// <summary>
        /// 子UI列表
        /// </summary>
        public List<BaseUI> subUIs = new List<BaseUI>();

        /// <summary>
        /// 临时子UI列表，用于批量操作
        /// </summary>
        public List<BaseUI> tmpSubUIs = new List<BaseUI>();

        /// <summary>
        /// 协程列表，用于管理协程
        /// </summary>
        public List<Coroutine> coroutines = new List<Coroutine>();

        /// <summary>
        /// 定时器对象列表，用于管理定时器
        /// </summary>
        public List<TimerObj> timerObjs = new List<TimerObj>();

        /// <summary>
        /// 资源句柄列表，用于管理所有加载的资源
        /// </summary>
        public List<IBaseAssetHandle> handles = new List<IBaseAssetHandle>();

        /// <summary>
        /// 显示计数器，用于同步显示操作
        /// </summary>
        private int showCount;

        /// <summary>
        /// 隐藏计数器，用于同步隐藏操作
        /// </summary>
        private int hideCount;

        /// <summary>
        /// 取消令牌源，用于取消异步操作
        /// </summary>
        public EasyCancellationToken token;

        /// <summary>
        /// 获取UI名称，默认为类名
        /// </summary>
        /// <returns>UI名称</returns>
        public virtual string GetUIName()
        {
            return GetType().Name;
        }

        /// <summary>
        /// 初始化UI组件，设置取消令牌并订阅事件
        /// </summary>
        public virtual void Awake()
        {
            token = new EasyCancellationToken();
            EventMgr.Instance.SubscribeByTarget(this);
            AddUIState(UIState.Awake);
            subUIs.ForEach(subUI => { if (!subUI.GetUIState(UIState.Awake)) subUI.Awake(); });
        }

        /// <summary>
        /// 启动UI组件，实例化GameObject或创建新的GameObject
        /// </summary>
        public virtual void Start()
        {
            if (handle != null)
            {
                gameObject = handle.Instantiate();
            }
            
            if (gameObject == null)
            {
                gameObject = new GameObject(GetType().Name);
            }

            AddUIState(UIState.Start);

            subUIs.ForEach(subUI =>
            { 
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Start))
                { 
                    subUI.Start(); 
                }
            });
        }

        /// <summary>
        /// 显示UI组件及其子UI
        /// </summary>
        /// <param name="callback">显示完成后的回调函数</param>
        public virtual void Show(Action callback = null)
        {
            if (GetUIState(UIState.Show))
            {
                callback?.Invoke();
                return;
            }
            AddUIState(UIState.Show);
            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            showCount = tmpSubUIs.Count;
            tmpSubUIs.ForEach(subUI => 
            { 
                if (subUI.GetUIState(UIState.Start))
                { 
                    subUI.Show(() =>
                    { 
                        --showCount; 
                        if (showCount == 0)
                        { 
                            gameObject.SetActive(true); 
                            callback?.Invoke();
                        }
                    }); 
                }
            });
        }

        /// <summary>
        /// 隐藏UI组件及其子UI
        /// </summary>
        /// <param name="callback">隐藏完成后的回调函数</param>
        public virtual void Hide(Action callback = null)
        {
            if (!GetUIState(UIState.Show))
            {
                callback?.Invoke(); 
                return;
            }
            SubUIState(UIState.Show);
            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            hideCount = tmpSubUIs.Count;
            tmpSubUIs.ForEach(subUI => 
            { 
                if (subUI.GetUIState(UIState.Start))
                { 
                    subUI.Hide(() =>
                    { 
                        --hideCount; 
                        if (hideCount == 0)
                        { 
                            gameObject.SetActive(false); 
                            callback?.Invoke(); 
                        }
                    }); 
                }
            });
        }

        /// <summary>
        /// 更新UI组件及其子UI
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        public virtual void Update(float deltaTime)
        {
            if (GetUIState(UIState.Start))
            {
                tmpSubUIs.Clear();
                tmpSubUIs.AddRange(subUIs);
                tmpSubUIs.ForEach(subUI => 
                { 
                    subUI.Update(deltaTime);
                });
            }
        }

        /// <summary>
        /// 销毁UI组件及其子UI
        /// </summary>
        public virtual void Destroy()
        {
            AddUIState(UIState.Destroy);
            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            tmpSubUIs.ForEach(subUI =>
            { 
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Destroy))
                { 
                    subUI.Destroy(); 
                }
            });

            foreach (var routine in coroutines)
            {
                CoroutineMgr.Instance.StopCoroutine(routine);
            }
            coroutines.Clear();

            TimerMgr.Instance.ClearByTarget(this);

            token.Cancel();
            token = null;

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
        }

        /// <summary>
        /// 标记UI组件已销毁
        /// </summary>
        public void Destroyed()
        {
            AddUIState(UIState.Destroyed);
            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            tmpSubUIs.ForEach(subUI =>
            { 
                if (subUI.GetUIState(UIState.Destroy) && !subUI.GetUIState(UIState.Destroyed))
                { 
                    subUI.Destroyed(); 
                }
            });
            subUIs.Clear();
            EventMgr.Instance.UnSubscribeByTarget(this);
        }

        /// <summary>
        /// 设置预制体路径，用于实例化UI
        /// </summary>
        /// <param name="path">预制体路径</param>
        public void SetPrefabPath(string path)
        {
            if (gameObject != null)
            {
                throw new System.Exception("UI错误：不能在已有GameObject的情况下设置预制体路径");
            }
            handle = AssetsMgr.Instance.LoadAsset<GameObject>(path);
        }

        /// <summary>
        /// 设置现有的GameObject作为UI的根对象
        /// </summary>
        /// <param name="gObj">现有的GameObject</param>
        public void SetGameObject(GameObject gObj)
        {
            if (handle != null)
            {
                throw new System.Exception("UI错误：不能在已有资源句柄的情况下设置GameObject");
            }
            gameObject = gObj;
        }

        /// <summary>
        /// 添加子UI
        /// </summary>
        /// <param name="subUI">子UI实例</param>
        public void AddSubUI(BaseUI subUI)
        {
            subUIs.Add(subUI);
            if (GetUIState(UIState.Awake))
            {
                if (!subUI.GetUIState(UIState.Awake))
                {
                    subUI.Awake();
                }
            }

            if (GetUIState(UIState.Start))
            {
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Start))
                {
                    subUI.Start();
                }
            }

            if (GetUIState(UIState.Show))
            {
                subUI.Show();
            }
            else
            {
                subUI.Hide();
            }

            if (GetUIState(UIState.Destroy))
            {
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Destroy))
                {
                    subUI.Destroy();
                }
            }

            if (GetUIState(UIState.Destroyed))
            {
                if (subUI.GetUIState(UIState.Destroy) && !subUI.GetUIState(UIState.Destroyed))
                {
                    subUI.Destroyed();
                }
            }
        }

        /// <summary>
        /// 移除子UI
        /// </summary>
        /// <param name="subUI">要移除的子UI实例</param>
        public void RemoveUIWidget(BaseUI subUI)
        {
            if (subUIs.Contains(subUI))
            {
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Destroy))
                {
                    subUI.Destroy();
                }
                if (subUI.GetUIState(UIState.Destroy) && !subUI.GetUIState(UIState.Destroyed))
                {
                    subUI.Destroyed();
                }
                subUIs.Remove(subUI);
            }
        }

        /// <summary>
        /// 添加UI状态
        /// </summary>
        /// <param name="state">要添加的状态</param>
        public void AddUIState(UIState state)
        {
            if ((uiState & state) == UIState.None)
            {
                uiState |= state;
            }
        }

        /// <summary>
        /// 移除UI状态
        /// </summary>
        /// <param name="state">要移除的状态</param>
        public void SubUIState(UIState state)
        {
            if ((uiState & state) != UIState.None)
            {
                uiState &= ~state;
            }
        }

        /// <summary>
        /// 检查UI是否处于指定状态
        /// </summary>
        /// <param name="state">要检查的状态</param>
        /// <returns>是否处于指定状态</returns>
        public bool GetUIState(UIState state)
        {
            return (uiState & state) != UIState.None;
        }

        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="item">协程迭代器</param>
        /// <returns>启动的协程</returns>
        public Coroutine StartCoroutine(IEnumerator item)
        {
            var routine = CoroutineMgr.Instance.StartCoroutine(item);
            coroutines.Add(routine);
            return routine;
        }

        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="routine">要停止的协程</param>
        public void StopCoroutine(Coroutine routine)
        {
            CoroutineMgr.Instance.StopCoroutine(routine);
            coroutines.Remove(routine);
        }

        /// <summary>
        /// 注册定时器
        /// </summary>
        /// <param name="timerCallBack">定时器回调函数</param>
        /// <param name="args">回调参数</param>
        /// <param name="priority">优先级</param>
        /// <param name="afterTime">延迟时间（秒）</param>
        /// <param name="loop">循环次数（0表示无限循环）</param>
        /// <param name="loopInterval">循环间隔（秒）</param>
        public void RegisterTimer(TimerCallBack timerCallBack, object[] args, int priority = 1, float afterTime = 0, int loop = 0, float loopInterval = 1)
        {
            TimerMgr.Instance.Register(this, timerCallBack, args, priority, DateTime.Now.Ticks + (long)(afterTime * TimeSpan.TicksPerSecond), loop, (long)(loopInterval * TimeSpan.TicksPerSecond));
        }

        /// <summary>
        /// 加载单个资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public IBaseAssetHandle LoadAsset(string path)
        {
            IBaseAssetHandle handle = AssetsMgr.Instance.LoadAsset<UnityEngine.Object>(path);
            handles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 加载多个资源
        /// </summary>
        /// <param name="paths">资源路径列表</param>
        /// <returns>资源句柄</returns>
        public IBaseAssetHandle LoadAssets(List<string> paths)
        {
            IBaseAssetHandle handle = AssetsMgr.Instance.LoadAssetsByPath<UnityEngine.Object>(paths);
            handles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 加载单个原始资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public IBaseAssetHandle LoadRawAsset(string path)
        {
            IBaseAssetHandle handle = AssetsMgr.Instance.LoadRawAsset(path);
            handles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 加载多个原始资源
        /// </summary>
        /// <param name="paths">资源路径列表</param>
        /// <returns>资源句柄</returns>
        public IBaseAssetHandle LoadRawAssets(List<string> paths)
        {
            IBaseAssetHandle handle = AssetsMgr.Instance.LoadRawAssetsByPath(paths);
            handles.Add(handle);
            return handle;
        }
    }
}