using System;
using UnityEngine;

namespace Easy
{

    [Serializable]
    public class EasyRunnerConfig : EasyConfig
    {
        [Label("是否开启日志")]
        [SerializeField]
        public bool IsOpenThread;

        [Label("线程模式")]
        [SerializeField]
        public ThreadMode threadMode;
    }
}