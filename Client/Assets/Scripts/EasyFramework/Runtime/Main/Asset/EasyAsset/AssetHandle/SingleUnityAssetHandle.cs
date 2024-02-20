using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Easy.EasyAsset
{
    /// <summary>
    /// 单资源的句柄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingleUnityAssetHandle<T> : BaseUnityAssetHandle, ISingleUnityAssetHandle<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string path;

        /// <summary>
        /// 加载结果
        /// </summary>
        public T result;

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            weakReferences.Clear();
            path = null;
            result = null;
        }

        /// <summary>
        /// 获取路径
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            return path;
        }

        /// <summary>
        /// 同步获取结果
        /// </summary>
        /// <returns></returns>
        public T GetResult()
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            if (!IsDone())
            {
                WaitForCompletion();
            }
            if( result == null)
            {
                result = loader.LoadUnityAssetInternal<T>(path);
            }
            return result;
        }

        /// <summary>
        /// 异步获取结果
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<T> GetResultAsync(Action<T> action = null)
        {
            if (isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            var overTimeTask = Task.Run(async () => { await Task.Delay(BaseUnityAssetHandle.instanceOverTime); });
            if (IsDone())
            {
                taskCompletionSource.SetResult(true);
            }
            else
            {
                taskCompletionSources.Add(taskCompletionSource);
            }
            await Task.WhenAny(taskCompletionSource.Task, overTimeTask);
            overTimeTask.Dispose();
            T t = null;
            if (taskCompletionSource.Task.IsCompleted && taskCompletionSource.Task.Result)
            {
                t = GetResult();
            }
            action?.Invoke(t);
            return t;
        }
        
        /// <summary>
        /// 同步实例对象
        /// </summary>
        /// <returns></returns>
        public T Instantiate()
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            if (!IsDone())
            {
                WaitForCompletion();
            }
            T instance = UnityEngine.Object.Instantiate(GetResult());
            weakReferences.Add(new WeakReference<UnityEngine.Object>(instance));
            return instance;
        }

        /// <summary>
        /// 异步实例对象
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<T> InstantiateAsync(Action<T> action = null)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }

            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            if(IsDone())
            {
                taskCompletionSource.SetResult(true);
            }
            else
            {
                var overTimeTask = Task.Run(async () => { await Task.Delay(BaseUnityAssetHandle.instanceOverTime); });
                taskCompletionSources.Add(taskCompletionSource);
                await Task.WhenAny(taskCompletionSource.Task, overTimeTask);
                overTimeTask.Dispose();
            }
            T t = null;
            if (taskCompletionSource.Task.IsCompleted && taskCompletionSource.Task.Result)
            {
                t = Instantiate();
            }
            action?.Invoke(t);
            return t;
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="t"></param>
        public void ReleaseInstance(T t)
        {
            base.ReleaseInstance(t);
        }
    }
}