using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace Easy.AA
{
    /// <summary>
    /// 多资源的句柄
    /// </summary>
    public class MultiUnityAssetHandle : BaseUnityAssetHandle, IMultiUnityAssetHandle
    {
        /// <summary>
        /// 加载的多资源路径
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public List<string> paths = new List<string>();
        
        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            paths.Clear();
            base.Reset();
        }

        /// <summary>
        /// 获取加载资源路径
        /// </summary>
        /// <returns></returns>
        public List<string> GetPaths()
        {
            return paths;
        }

        /// <summary>
        /// 获取加载结果
        /// </summary>
        /// <returns></returns>
        public List<UnityEngine.Object> GetResult()
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }

            if (!IsDone())
            {
                WaitForCompletion();
            }
            return (List<UnityEngine.Object>)result.Task.Result;
        }

        /// <summary>
        /// 异步获取加载结果
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async EasyTask<List<UnityEngine.Object>> GetResultAsync(System.Action<List<UnityEngine.Object>> action = null)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            if (other.IsValid())
            {
                await other.Task;
            }
            if (result.IsValid())
            {
                await result.Task;
            }
            List<UnityEngine.Object> t = (List<UnityEngine.Object>)result.Result;
            action?.Invoke(t);
            return t;

        }

        /// <summary>
        /// 实例对象
        /// </summary>
        /// <returns></returns>
        public List<UnityEngine.Object> Instantiates()
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }

            if (!IsDone())
            {
                WaitForCompletion();
            }
            List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
            List<UnityEngine.Object> results = (List<UnityEngine.Object>)result.Result;
            for (int i = 0; i < results.Count; i++)
            {
                UnityEngine.Object obj = UnityEngine.Object.Instantiate(results[i]);
                objects.Add(obj);
                weakReferences.Add(new System.WeakReference<UnityEngine.Object>(obj));
            }
            return objects;
        }

        /// <summary>
        /// 异步实例对象
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async EasyTask<List<UnityEngine.Object>> InstantiatesAsync(System.Action<List<UnityEngine.Object>> action = null)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            if(other.IsValid())
            {
                await other.Task;
            }
            if(result.IsValid())
            {
                await result.Task;
            }
            List<UnityEngine.Object> t = Instantiates();
            action?.Invoke(t);
            return t;
        }
        
        /// <summary>
        /// 实例化对象
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public UnityEngine.Object Instantiate(string path)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }

            if (!IsDone())
            {
                WaitForCompletion();
            }

            int index = paths.IndexOf(path);
            List<UnityEngine.Object> temps = GetResult();
            UnityEngine.Object obj = UnityEngine.Object.Instantiate(temps[index]);
            weakReferences.Add(new WeakReference<UnityEngine.Object>(obj));
            return obj;
        }

        /// <summary>
        /// 异步实例化对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async EasyTask<UnityEngine.Object> InstantiateAsync(string path, Action<UnityEngine.Object> action = null)
        {
            if(isInPool)
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

            int index = paths.IndexOf(path);
            List<UnityEngine.Object> temps = GetResult();
            UnityEngine.Object obj = UnityEngine.Object.Instantiate(temps[index]);
            weakReferences.Add(new WeakReference<UnityEngine.Object>(obj));
            return obj;
        }

        /// <summary>
        /// 释放多个对象
        /// </summary>
        /// <param name="t"></param>
        public void ReleaseInstances(List<UnityEngine.Object> t)
        {
            foreach (var item in t)
            {
                ReleaseInstance(item);
            }
        }
    }
}