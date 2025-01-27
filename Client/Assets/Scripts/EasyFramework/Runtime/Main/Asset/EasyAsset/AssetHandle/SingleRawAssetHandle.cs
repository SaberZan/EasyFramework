using System;
using Cysharp.Threading.Tasks;

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
        public async UniTask<byte[]> GetResultAsync(Action<byte[]> action = null)
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

            var taskCompletionSource = new UniTaskCompletionSource<bool>();
            taskCompletionSources.Add(taskCompletionSource);
            var overTimeTask = UniTask.Delay(BaseUnityAssetHandle.instanceOverTime);
            await UniTask.WhenAny(taskCompletionSource.Task, overTimeTask);

            byte[] t = null;
            if (taskCompletionSource.Task.Status == UniTaskStatus.Succeeded)
            {
                t = GetResult();
            }

            action?.Invoke(t);
            return t;
        }
        
    }
}