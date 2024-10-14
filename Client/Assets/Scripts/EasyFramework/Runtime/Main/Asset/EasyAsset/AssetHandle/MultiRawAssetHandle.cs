using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Easy.EasyAsset
{
    /// <summary>
    /// 单资源的句柄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultiRawAssetHandle : BaseAssetHandle, IMultiRawAssetHandle
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public List<string> paths = new List<string>();

        /// <summary>
        /// 加载结果
        /// </summary>
        public List<byte[]> result = null;

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            paths.Clear();
            result = null;
        }

        /// <summary>
        /// 获取路径
        /// </summary>
        /// <returns></returns>
        public List<string> GetPaths()
        {
            return paths;
        }

        /// <summary>
        /// 同步获取结果
        /// </summary>
        /// <returns></returns>
        public List<byte[]> GetResult()
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
                result = new List<byte[]>(paths.Count);
                foreach (string path in paths)
                {
                    var item = loader.LoadRawAssetInternal(path);
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// 异步获取结果
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<List<byte[]>> GetResultAsync(Action<List<byte[]>> action = null)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();;
            if(IsDone())
            {
                taskCompletionSource.SetResult(true);
            }
            else
            {
                var overTimeTask = Task.Run(async () => { await UniTask.Delay(BaseUnityAssetHandle.instanceOverTime); });
                taskCompletionSources.Add(taskCompletionSource);
                await Task.WhenAny(taskCompletionSource.Task, overTimeTask);
                overTimeTask.Dispose();
            }
            List<byte[]> t = null;
            if (taskCompletionSource.Task.IsCompleted && taskCompletionSource.Task.Result)
            {
                t = GetResult();
            }
            action?.Invoke(t);
            return t;
        }
        
    }
}