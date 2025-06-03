using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace Easy.AA
{
    /// <summary>
    /// 单资源的句柄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultiRawAssetHandle : BaseRawAssetHandle, IMultiRawAssetHandle
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public List<string> paths = new List<string>();

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            paths.Clear();
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
            
            List<byte[]> bytes = new List<byte[]>();
            foreach(TextAsset textAsset in (List<UnityEngine.TextAsset>)result.Task.Result)
            {
                bytes.Add(textAsset.bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 异步获取结果
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async EasyTask<List<byte[]>> GetResultAsync(Action<List<byte[]>> action = null)
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

            List<byte[]> bytes = new List<byte[]>();
            foreach (TextAsset textAsset in (List<UnityEngine.TextAsset>)result.Task.Result)
            {
                bytes.Add(textAsset.bytes);
            }
            return bytes;
        }
        
    }
}