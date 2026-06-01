using System;
using UnityEngine;

namespace Easy
{
    [Serializable]
    public class EasyLoggerConfig : EasyConfig
    {
        [Label("是否开启日志")]
        [SerializeField]
        public bool IsLogOpen;

        [Label("打印日志的tag")]
        [SerializeField]
        public string[] openTags;

        [Label("是否写文件")]
        [SerializeField]
        public bool IsWriteFile;

        [Label("是否Unity打印")]
        [SerializeField]
        public bool IsUnityLog;
    }
}