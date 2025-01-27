using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Easy.EasyAsset
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
        /// 加载的多资源结果
        /// </summary>
        public List<UnityEngine.Object> result = null;

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            weakReferences.Clear();
            paths.Clear();
            result = null;
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
            if( result == null)
            {
                if (IsDone())
                {
                    result = new List<UnityEngine.Object>(paths.Count);
                    foreach (string path in paths)
                    {
                        var item = loader.LoadUnityAssetInternal<UnityEngine.Object>(path);
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 异步获取加载结果
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async UniTask<List<UnityEngine.Object>> GetResultAsync(System.Action<List<UnityEngine.Object>> action = null)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            if (IsDone())
            {
                var result = GetResult();
                action?.Invoke(result);
                return result;
            }
            UniTaskCompletionSource<bool> taskCompletionSource = new UniTaskCompletionSource<bool>();;
            taskCompletionSources.Add(taskCompletionSource);
            var overTimeTask = UniTask.Delay(BaseUnityAssetHandle.instanceOverTime);
            await UniTask.WhenAny(taskCompletionSource.Task, overTimeTask);
            List<UnityEngine.Object> t = null;
            if (taskCompletionSource.Task.Status == UniTaskStatus.Succeeded)
            {
                t = GetResult();
            }
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
            List<UnityEngine.Object> objects = null;
            if (IsDone())
            {
                objects = new List<UnityEngine.Object>();
                List<UnityEngine.Object> temps = GetResult();
                for (int i = 0; i < temps.Count; i++)
                {
                    UnityEngine.Object obj = UnityEngine.Object.Instantiate(temps[i]);
                    objects.Add(obj);
                    weakReferences.Add(new WeakReference<UnityEngine.Object>(obj));
                }
            }
            return objects;
        }

        /// <summary>
        /// 异步实例对象
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async UniTask<List<UnityEngine.Object>> InstantiatesAsync(System.Action<List<UnityEngine.Object>> action = null)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }

            if (IsDone())
            {
                var result = Instantiates();
                action?.Invoke(result);
                return result;
            }
            UniTaskCompletionSource<bool> taskCompletionSource = new UniTaskCompletionSource<bool>();;
            taskCompletionSources.Add(taskCompletionSource);
            var overTimeTask = UniTask.Delay(BaseUnityAssetHandle.instanceOverTime);
            await UniTask.WhenAny(taskCompletionSource.Task, overTimeTask);
            List<UnityEngine.Object> t = null;
            if (taskCompletionSource.Task.Status == UniTaskStatus.Succeeded)
            {
                t = Instantiates();
            }
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
        public async UniTask<UnityEngine.Object> InstantiateAsync(string path, Action<UnityEngine.Object> action = null)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }

            if (IsDone())
            {
                var result = Instantiate(path);
                action?.Invoke(result);
                return result;
            }
            UniTaskCompletionSource<bool> taskCompletionSource = new UniTaskCompletionSource<bool>();;
            taskCompletionSources.Add(taskCompletionSource);
            var overTimeTask = UniTask.Delay(BaseUnityAssetHandle.instanceOverTime);
            await UniTask.WhenAny(taskCompletionSource.Task, overTimeTask);
            UnityEngine.Object t = null;
            if (taskCompletionSource.Task.Status == UniTaskStatus.Succeeded)
            {
                t = Instantiate(path);
            }
            action?.Invoke(t);
            return t;
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