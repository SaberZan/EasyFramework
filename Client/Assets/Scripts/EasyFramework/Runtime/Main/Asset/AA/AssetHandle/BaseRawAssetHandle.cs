using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Easy.AA
{
    /// <summary>
    /// 句柄基类
    /// </summary>
    public class BaseRawAssetHandle : BaseAssetHandle
    {


        /// <summary>
        /// 开始下载
        /// </summary>
        public override void Start()
        {

        }

        /// <summary>
        /// 重新使用
        /// </summary>
        public override void Reuse()
        {
            base.Reuse();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            base.Reset();
        }

    }
}