using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Easy.EasyAsset
{
    
    /// <summary>
    /// 每一个ab包的信息
    /// </summary>
    [Serializable]
    public class EasyAssetBundleInfo
    {
        /// <summary>
        /// ab包种类
        /// </summary>
        public EasyAssetBundleType easyAssetBundleType;
        
        /// <summary>
        /// AB包名
        /// </summary>
        public string abName;

        /// <summary>
        /// md5
        /// </summary>
        public string md5;
        
        /// <summary>
        /// AB包大小
        /// </summary>
        public long size;

        /// <summary>
        /// crc标志
        /// </summary>
        public uint crc;

        /// <summary>
        /// 被什么Package使用
        /// </summary>
        public List<string> packages;

        /// <summary>
        /// ab包位置
        /// </summary>
        public Location location;

        /// <summary>
        /// 是否加密
        /// </summary>
        public bool isEncrypt;

        /// <summary>
        /// 是否启动必须有
        /// </summary>
        public DownloadPriority abDownloadPriority;

        public EasyAssetBundleInfo()
        {
            md5 = "";
            packages = new List<string>();
        }
    }
}