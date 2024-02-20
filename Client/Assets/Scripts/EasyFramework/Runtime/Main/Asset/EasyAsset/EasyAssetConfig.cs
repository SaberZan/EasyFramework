using System;
using UnityEngine;
//using Toolbox.Editor.Wizards;
//using Toolbox.Editor;

namespace Easy.EasyAsset
{
    /// <summary>
    /// 资源开关配置
    /// </summary
    [Serializable]
    public class EasyAssetConfig : EasyConfig
    {
        [Help("是否开启热更", UnityMessageType.Info)]
        [SerializeField]
        public bool hotUpdate = false;

        [Help("是否开启强制热更", UnityMessageType.Info)]
        [SerializeField]
        public bool forceHotUpdate = false;

        [Help("更新最大的下载数量", UnityMessageType.Info)]
        [SerializeField]
        public int maxDownloadNum = 5;

        [Help("请求版本地址", UnityMessageType.Info)]
        [SerializeField]
        public string requestVersionUrl = "";

        [Help("资源实例超时", UnityMessageType.Info)]
        [SerializeField]
        public int instanceOverTime = 3000;
    }
}