using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Easy.EasyAsset
{
    /// <summary>
    /// 句柄基类
    /// </summary>
    public class BaseAssetHandle
    {
        /// <summary>
        /// 资源加载器
        /// </summary>
        public EasyAssetsLoader loader;
        
        /// <summary>
        /// 加载的资源依赖了哪些ab
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        public List<int> dependAB = new List<int>();

        /// <summary>
        /// 资源加载完成列表
        /// </summary>
        /// <returns></returns>
        public List<UniTaskCompletionSource<bool>> taskCompletionSources = new List<UniTaskCompletionSource<bool>>();

        /// <summary>
        /// 是否已加载完成
        /// </summary>
        public bool isDone;

        /// <summary>
        /// 是否加载失败
        /// </summary>
        public bool downloadFail;

        /// <summary>
        /// 这个句柄再缓冲池中的标记
        /// </summary>
        public bool isInPool;
                
        /// <summary>
        /// 实例超时
        /// </summary>
        public static int instanceOverTime;

        /// <summary>
        /// 开始下载
        /// </summary>
        public virtual void Start()
        {
            for(int i = dependAB.Count - 1; i >= 0; --i)
            {
                loader.StartDownloadEasyAssetBundle(dependAB[i]);
            }
        }

        /// <summary>
        /// 是否下载完
        /// </summary>
        /// <returns></returns>
        public bool IsDone()
        {
            if(!isDone)
            {
                if(CheckComplete())
                {
                    isDone = true;
                }
            }
            return isDone;
        }

        /// <summary>
        /// 等待完成,阻塞进程
        /// </summary>
        public void WaitForCompletion()
        {
            long startTick = DateTime.Now.Ticks;
            while(!isDone)
            {
                if(downloadFail)
                {
                    break;
                }
                
                if(IsDone())
                {
                    break;
                }
                
                long nowTick = DateTime.Now.Ticks;
                if (nowTick - startTick > BaseUnityAssetHandle.instanceOverTime)
                {
                    break;
                }
                
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 检测依赖资源是否加载完成
        /// </summary>
        /// <returns></returns>
        public bool CheckComplete()
        {
            for (int i = dependAB.Count - 1; i >= 0; --i)
            {
                if (!loader.CheckEasyAssetBundleCompleted(dependAB[i]))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 重新使用
        /// </summary>
        public virtual void Reuse()
        {
            isInPool = false;
            Easy.EventMgr.Instance.Subscribe(Const.DOWNLOAD_SUCCESS, this, OnABDownloadSuccess);
            Easy.EventMgr.Instance.Subscribe(Const.DOWNLOAD_FAIL, this, OnABDownloadFailed);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public virtual void Reset()
        {
            Easy.EventMgr.Instance.UnSubscribe(Const.DOWNLOAD_SUCCESS, this, OnABDownloadSuccess);
            Easy.EventMgr.Instance.UnSubscribe(Const.DOWNLOAD_FAIL, this, OnABDownloadFailed);
            dependAB.Clear();
            taskCompletionSources.Clear();
            loader = null;
            isDone = false;
            downloadFail = false;
            isInPool = true;
        }
        
        /// <summary>
        /// 有ab包下载完回调
        /// </summary>
        /// <param name="eventArg"></param>
        public void OnABDownloadSuccess(EventArg eventArg)
        {
            if(!isDone)
            {
                if(IsDone())
                {
                    for(int i = taskCompletionSources.Count - 1; i >= 0; ++i)
                    {
                        taskCompletionSources[i].TrySetResult(isDone);
                    }
                }
            }
        }

        /// <summary>
        /// 有ab包下载失败回调
        /// </summary>
        /// <param name="eventArg"></param>
        public void OnABDownloadFailed(EventArg eventArg)
        {
            DownloadEventArg arg = (DownloadEventArg)eventArg;
            loader.catalogs.GetEasyAssetBundleInfoByMD5(arg.md5, out int index);
            if(dependAB.Contains(index))
            {
                downloadFail = true;
            }
        }
    }
}