using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Easy.EasyAsset
{
    /// <summary>
    /// 资源信息
    /// </summary>
    [Serializable]
    public class EasyAssetInfo
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string asset;

        /// <summary>
        /// 属于哪个ab包
        /// </summary>
        public int abIndex;

        /// <summary>
        /// 依赖了哪些ab包
        /// </summary>
        public List<int> needABIndexes;

        /// <summary>
        /// 资源类型
        /// </summary>
        public int typeIndex;

        /// <summary>
        /// 修改时间标签
        /// </summary>
        public long changeTag;

        /// <summary>
        /// 是否是原始资源
        /// </summary>
        public bool isRaw;

        /// <summary>
        /// 偏移量
        /// </summary>
        public long offset;

        /// <summary>
        /// 大小
        /// </summary>
        public long size;


    }
}