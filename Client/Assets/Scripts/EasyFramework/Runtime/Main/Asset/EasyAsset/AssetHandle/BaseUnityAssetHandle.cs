using System;
using System.Collections.Generic;


namespace Easy.EasyAsset
{
    /// <summary>
    /// 句柄基类
    /// </summary>
    public class BaseUnityAssetHandle : BaseAssetHandle
    {
        /// <summary>
        /// 存储被实例出来的资源引用
        /// </summary>
        /// <typeparam name="UnityEngine.Object"></typeparam>
        /// <returns></returns>
        public List<WeakReference<UnityEngine.Object>> weakReferences = new List<WeakReference<UnityEngine.Object>>(2);

        /// <summary>
        /// 是否自动释放
        /// </summary>
        private bool isAutoRelease;
        
        
        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            weakReferences.Clear();
            base.Reset();
        }
        
        /// <summary>
        /// 释放实例
        /// </summary>
        /// <param name="t"></param>
        public void ReleaseInstance(UnityEngine.Object t)
        {
            bool hasReference = false;
            for (int i = weakReferences.Count - 1; i >= 0; --i)
            {
                if (weakReferences[i].TryGetTarget(out UnityEngine.Object obj) && obj == t)
                {
                    weakReferences.RemoveAt(i);
                    hasReference = true;
                    break;
                }
            }
            if(!hasReference)
            {
                throw new Exception("obj is not instance from this handle");
                //Easy.EasyLogger.LogError("obj is not instance from this handle");
            }
            UnityEngine.Object.Destroy(t);
        }

        /// <summary>
        /// 实例的个数
        /// </summary>
        /// <returns></returns>
        public int AliveCount()
        {
            int count = 0;
            for (int i = weakReferences.Count - 1; i >= 0; --i)
            {
                if (weakReferences[i].TryGetTarget(out UnityEngine.Object obj) && obj != null)
                {
                    ++count;
                }
            }
            return count;
        }

        /// <summary>
        /// 是否自动释放
        /// </summary>
        /// <returns></returns>
        public bool IsAutoRelease()
        {
            return isAutoRelease;
        }

        /// <summary>
        /// 设置自动释放
        /// </summary>
        /// <param name="value"></param>
        public void SetAutoRelease(bool value)
        {
            isAutoRelease = value;
        }

    }
}