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
        Enable = 1 << 2,
        Disable = 1 << 3,
        Destroy = 1 << 4,
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
        /// 是否异步加载
        /// </summary>
        public bool loadAsync = false;

        /// <summary>
        /// 承接所有的gameobject
        /// </summary>
        public GameObject baseGameObject;
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
        /// 打开UI
        /// </summary>
        public void Show()
        {
            Awake();
            Enable();
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public void Hide()
        {
            Awake();
            Disable();
        }

        /// <summary>
        /// 初始化UI组件，设置取消令牌并订阅事件
        /// </summary>
        public void Awake()
        {
            if (GetUIState(UIState.Awake))
            {
                return;
            }
            AddUIState(UIState.Awake);

            token = new EasyCancellationToken();
            EventMgr.Instance.SubscribeByTarget(this);

            if (baseGameObject == null)
            {
                baseGameObject = new GameObject(GetType().Name);
            }

            OnCreate();

            if (handle != null)
            {
                if (loadAsync)
                {
                    var task = handle.InstantiateAsync();
                    task.OnCompleted(() =>
                    {
                        gameObject = task.GetResult();
                        gameObject.transform.SetParent(baseGameObject.transform, false);
                        OnStart();
                        if (GetUIState(UIState.Enable))
                        {
                            OnEnable();
                        }
                        if (GetUIState(UIState.Disable))
                        {
                            OnDisable();
                        }
                    });
                }
                else
                {
                    gameObject = handle.Instantiate();
                    OnStart();
                }
            }

            if (gameObject != null)
            {
                gameObject.transform.SetParent(baseGameObject.transform, false);
                OnStart();
            }
            
            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            tmpSubUIs.ForEach(subUI =>
            {
                if (!subUI.GetUIState(UIState.Awake))
                    subUI.Awake();
            });
        }


        /// <summary>
        /// 显示UI组件及其子UI
        /// </summary>
        /// <param name="callback">显示完成后的回调函数</param>
        public void Enable()
        {
            if (GetUIState(UIState.Enable) && !GetUIState(UIState.Disable))
            {
                return;
            }
            AddUIState(UIState.Enable);
            RemoveUIState(UIState.Disable);

            if (gameObject != null)
            {
                OnEnable();
            }

            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            tmpSubUIs.ForEach(subUI =>
            {
                if (subUI.GetUIState(UIState.Awake))
                {
                    subUI.Enable();
                }
            });
            baseGameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏UI组件及其子UI
        /// </summary>
        /// <param name="callback">隐藏完成后的回调函数</param>
        public void Disable()
        {
            if (GetUIState(UIState.Disable))
            {
                return;
            }
            AddUIState(UIState.Disable);
   
            if (gameObject != null)
            {
                OnDisable();
            }

            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            tmpSubUIs.ForEach(subUI =>
            {
                if (subUI.GetUIState(UIState.Awake))
                {
                    subUI.Disable();
                }
            });
            baseGameObject.SetActive(false); 
        }

        /// <summary>
        /// 更新UI组件及其子UI
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        public virtual void Update(float deltaTime)
        {
            if (GetUIState(UIState.Awake))
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
        public void Destroy()
        {
            if (GetUIState(UIState.Destroy))
            {
                return;
            }
            AddUIState(UIState.Destroy);


            foreach (var routine in coroutines)
            {
                CoroutineMgr.Instance.StopCoroutine(routine);
            }
            coroutines.Clear();

            TimerMgr.Instance.ClearByTarget(this);
            timerObjs.Clear();

            token.Cancel();
            token = null;


            tmpSubUIs.Clear();
            tmpSubUIs.AddRange(subUIs);
            tmpSubUIs.ForEach(subUI =>
            {
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Destroy))
                {
                    subUI.Destroy();
                }
            });
            subUIs.Clear();

            EventMgr.Instance.UnSubscribeByTarget(this);

            foreach (var handle in handles)
            {
                AssetsMgr.Instance.Release(handle);
            }
            handles.Clear();

            if (handle != null)
            {
                if (gameObject != null)
                {
                    OnDestroy();
                    handle.ReleaseInstance(gameObject);
                    gameObject = null;
                }
                AssetsMgr.Instance.Release(handle);
            }
            else
            {
                if (gameObject != null)
                {
                    OnDestroy();
                    GameObject.Destroy(gameObject);
                    gameObject = null;
                }
            }
            GameObject.Destroy(baseGameObject);
        }

        /// <summary>
        /// 设置预制体路径，用于实例化UI
        /// </summary>
        /// <param name="path">预制体路径</param>
        public void SetPrefabPath(string path, bool async = false)
        {
            if (gameObject != null)
            {
                throw new System.Exception("UI错误：不能在已有GameObject的情况下设置预制体路径");
            }
            handle = AssetsMgr.Instance.LoadAsset<GameObject>(path);
            loadAsync = async;
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

            if (GetUIState(UIState.Enable))
            {
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Enable))
                {
                    subUI.Enable();
                }
            }

            if (GetUIState(UIState.Disable))
            {
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Disable))
                {
                    subUI.Disable();
                }
            }

            if (GetUIState(UIState.Destroy))
            {
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Destroy))
                {
                    subUI.Destroy();
                }
            }
        }

        /// <summary>
        /// 移除子UI
        /// </summary>
        /// <param name="subUI">要移除的子UI实例</param>
        public void RemoveSubUI(BaseUI subUI)
        {
            if (subUIs.Contains(subUI))
            {
                if (subUI.GetUIState(UIState.Awake) && !subUI.GetUIState(UIState.Destroy))
                {
                    subUI.Destroy();
                }
                subUIs.Remove(subUI);
            }
        }

        /// <summary>
        /// 刚开始创建
        /// </summary>
        public virtual void OnCreate()
        {

        }

        /// <summary>
        /// 添加实例节点后调用
        /// </summary>
        public virtual void OnStart()
        {

        }

        /// <summary>
        /// 有实例节点后进前台调用
        /// </summary>
        public virtual void OnEnable()
        {

        }

        /// <summary>
        /// 有实例节点后进后台调用
        /// </summary>
        public virtual void OnDisable()
        {

        }

        /// <summary>
        /// 有实例节点后销毁调用
        /// </summary>
        public virtual void OnDestroy()
        {

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
        public void RemoveUIState(UIState state)
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
            timerObjs.Add(TimerMgr.Instance.Register(this, timerCallBack, args, priority, DateTime.Now.Ticks + (long)(afterTime * TimeSpan.TicksPerSecond), loop, (long)(loopInterval * TimeSpan.TicksPerSecond)));
        }

        /// <summary>
        /// 反注册定时器
        /// </summary>
        /// <param name="timerCallBack"></param>
        public void UnregisterTimer(TimerCallBack timerCallBack)
        {
            for (int i = timerObjs.Count - 1; i >= 0; --i)
            {
                if (timerObjs[i].callback == timerCallBack)
                {
                    TimerMgr.Instance.UnRegister(timerObjs[i]);
                    timerObjs.RemoveAt(i);
                }
            }
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