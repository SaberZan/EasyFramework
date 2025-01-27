using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Easy.AA
{
    /// <summary>
    /// 单资源的句柄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingleUnityAssetHandle<T> : BaseUnityAssetHandle, ISingleUnityAssetHandle<T> where T : UnityEngine.Object
    {
        public string path;

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
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
            if (isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            if (!IsDone())
            {
                WaitForCompletion();
            }
            return (T)result.Result;
        }

        /// <summary>
        /// 异步获取结果
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async UniTask<T> GetResultAsync(Action<T> action = null)
        {
            if (isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            if (!IsDone())
            {
                if (other.IsValid())
                {
                    await other.Task;
                }
                if (result.IsValid())
                {
                    await result.Task;
                }
            }
            T t = (T)result.Result;
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
        public async UniTask<T> InstantiateAsync(Action<T> action = null)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            if (!IsDone())
            {
                if(other.IsValid())
                {
                    await other.Task;
                }
                if (result.IsValid())
                {
                    await result.Task;
                }
            }
            T instance = UnityEngine.Object.Instantiate(GetResult());
            weakReferences.Add(new WeakReference<UnityEngine.Object>(instance));
            action?.Invoke(instance);
            return instance;
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