using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Easy
{
    public class EasyMono : MonoBehaviour
    {

        /// <summary>
        /// 协程列表，用于管理协程
        /// </summary>
        public List<Coroutine> coroutines = new List<Coroutine>();

        /// <summary>
        /// 定时器对象列表，用于管理定时器
        /// </summary>
        public List<SchedulerObj> timerObjs = new List<SchedulerObj>();

        /// <summary>
        /// 资源句柄列表，用于管理所有加载的资源
        /// </summary>
        public List<IBaseAssetHandle> handles = new List<IBaseAssetHandle>();

        /// <summary>
        /// 取消令牌源，用于取消异步操作
        /// </summary>
        public EasyCancellationToken token;

        /// <summary>
        /// 初始化UI组件，设置取消令牌并订阅事件
        /// </summary>
        protected virtual void Awake()
        {
            token = new EasyCancellationToken();
            EventMgr.Instance.SubscribeByTarget(this);
        }


        protected virtual void Start()
        {

        }

        /// <summary>
        /// 销毁UI组件及其子UI
        /// </summary>
        protected virtual void OnDestroy()
        {

            foreach (var routine in coroutines)
            {
                CoroutineMgr.Instance.StopCoroutine(routine);
            }
            coroutines.Clear();

            SchedulerMgr.Instance.ClearByTarget(this);

            token.Cancel();
            token = null;

            foreach (var handle in handles)
            {
                AssetsMgr.Instance.Release(handle);
            }
            handles.Clear();

            EventMgr.Instance.UnSubscribeByTarget(this);
        }

        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="item">协程迭代器</param>
        /// <returns>启动的协程</returns>
        public new Coroutine StartCoroutine(IEnumerator item)
        {
            var routine = CoroutineMgr.Instance.StartCoroutine(item);
            coroutines.Add(routine);
            return routine;
        }

        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="routine">要停止的协程</param>
        public new void StopCoroutine(Coroutine routine)
        {
            CoroutineMgr.Instance.StopCoroutine(routine);
            coroutines.Remove(routine);
        }

        /// <summary>
        /// 注册定时器
        /// </summary>
        /// <param name="schedulerCallBack">定时器回调函数</param>
        /// <param name="args">回调参数</param>
        /// <param name="priority">优先级</param>
        /// <param name="afterTime">延迟时间（秒）</param>
        /// <param name="loop">循环次数（0表示无限循环）</param>
        /// <param name="loopInterval">循环间隔（秒）</param>
        public void RegisterScheduler(SchedulerCallBack schedulerCallBack, object[] args, int priority = 1, float afterTime = 0, int loop = 0, float loopInterval = 1)
        {
            SchedulerMgr.Instance.Register(this, schedulerCallBack, args, priority, DateTime.Now.Ticks + (long)(afterTime * TimeSpan.TicksPerSecond), loop, (long)(loopInterval * TimeSpan.TicksPerSecond));
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