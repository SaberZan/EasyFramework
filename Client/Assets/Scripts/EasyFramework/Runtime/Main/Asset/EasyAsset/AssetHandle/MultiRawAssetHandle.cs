using System;
using System.Collections.Generic;


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
        public async EasyTask<List<byte[]>> GetResultAsync(Action<List<byte[]>> action = null)
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
            EasyTask<bool> loadTask = EasyTask<bool>.Create();;
            loadTasks.Add(loadTask);
            var overTimeTask = EasyTaskRunner.Delay(BaseUnityAssetHandle.instanceOverTime);
            await EasyTaskRunner.WhenAny(loadTask, overTimeTask);
            List<byte[]> t = null;
            if (loadTask.EasyTaskState == EasyTaskState.Completed)
            {
                t = GetResult();
            }
            action?.Invoke(t);
            return t;
        }
        
    }
}