using System;
using System.Collections.Generic;

namespace Easy.EasyAsset
{
    /// <summary>
    /// 在所有AB包被的资源信息
    /// </summary>
    [Serializable]
    public class Version
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public long version;

        /// <summary>
        /// 版本号
        /// </summary>
        public long versionCode;

        /// <summary>
        /// 更新地址
        /// </summary>
        public string requestCatalogsUrl;
    }
}