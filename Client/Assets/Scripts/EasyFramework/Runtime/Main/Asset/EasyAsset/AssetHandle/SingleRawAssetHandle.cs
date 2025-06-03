using System;


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
            return result ?? (result = loader.LoadRawAssetInternal(path));
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
            if (IsDone())
            {
                var result = GetResult();
                action?.Invoke(result);
                return result;
            }

            var loadTask = EasyTask<bool>.Create();
            loadTasks.Add(loadTask);
            var overTimeTask = EasyTaskRunner.Delay(BaseUnityAssetHandle.instanceOverTime);
            await EasyTaskRunner.WhenAny(loadTask, overTimeTask);

            byte[] t = null;
            if (loadTask.EasyTaskState == EasyTaskState.Completed)
            {
                t = GetResult();
            }

            action?.Invoke(t);
            return t;
        }
        
    }
}