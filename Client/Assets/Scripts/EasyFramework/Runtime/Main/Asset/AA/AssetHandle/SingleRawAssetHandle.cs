using System;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace Easy.AA
{
    /// <summary>
    /// 单资源的句柄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingleRawAssetHandle : BaseRawAssetHandle, ISingleRawAssetHandle
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string path;

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            path = null;
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
            return ((TextAsset)result.Task.Result).bytes;
        }

        /// <summary>
        /// 异步获取结果
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async EasyTask<byte[]> GetResultAsync(Action<byte[]> action = null)
        {
            if(isInPool)
            {
                throw new Exception("handle 已被回收 !!");
            }
            
            if(other.IsValid())
            {
                await other.Task;
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
            return ((TextAsset)result.Task.Result).bytes;
        }
        
    }
}