using System;
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
    public class SingleRawAssetHandle : BaseAssetHandle, ISingleRawAssetHandle
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string path;

        /// <summary>
        /// 加载结果
        /// </summary>
        public byte[] result;

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
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
        public byte[] GetResult()
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
                result = loader.LoadRawAssetInternal(path);
            }
            return result;
        }

        /// <summary>
        /// 异步获取结果
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<byte[]> GetResultAsync(Action<byte[]> action = null)
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
                var overTimeTask = Task.Run(async () => { await UniTask.Delay(BaseUnityAssetHandle.instanceOverTime); });
                taskCompletionSources.Add(taskCompletionSource);
                await Task.WhenAny(taskCompletionSource.Task, overTimeTask);
                overTimeTask.Dispose();
            }
            byte[] t = null;
            if (taskCompletionSource.Task.IsCompleted && taskCompletionSource.Task.Result)
            {
                t = GetResult();
            }
            action?.Invoke(t);
            return t;
        }
        
    }
}