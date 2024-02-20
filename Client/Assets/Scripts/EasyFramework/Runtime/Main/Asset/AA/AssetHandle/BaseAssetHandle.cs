using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Easy.AA
{
    /// <summary>
    /// 句柄基类
    /// </summary>
    public class BaseAssetHandle
    {
        /// <summary>
        /// 资源加载器
        /// </summary>
        public AAAssetsLoader loader;

        public AsyncOperationHandle result;

        public AsyncOperationHandle other;

        /// <summary>
        /// 资源加载完成列表
        /// </summary>
        /// <returns></returns>
        public List<TaskCompletionSource<bool>> taskCompletionSources = new List<TaskCompletionSource<bool>>();

        /// <summary>
        /// 这个句柄再缓冲池中的标记
        /// </summary>
        public bool isInPool;
                
        /// <summary>
        /// 开始下载
        /// </summary>
        public virtual void Start()
        {

        }

        /// <summary>
        /// 是否下载完
        /// </summary>
        /// <returns></returns>
        public bool IsDone()
        {
            if (other.IsValid() && !other.IsDone)
            {
                return false;
            }
            if (result.IsValid() && !result.IsDone)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 等待完成,阻塞进程
        /// </summary>
        public void WaitForCompletion()
        {
            if (other.IsValid())
            {
                other.WaitForCompletion();
            }

            if (result.IsValid())
            {
                result.WaitForCompletion();
            }
        }

        /// <summary>
        /// 重新使用
        /// </summary>
        public virtual void Reuse()
        {
            isInPool = false;
        }

        /// <summary>
        /// 重置
        /// </summary>
        public virtual void Reset()
        {
            taskCompletionSources.Clear();
            loader = null;
            isInPool = true;
        }
    }
}