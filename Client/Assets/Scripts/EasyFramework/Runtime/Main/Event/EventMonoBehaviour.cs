namespace Easy
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 默认注册消息的MonoBehaviour
    /// </summary>
    public class EventMonoBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            EventMgr.Instance.SubscribeByTarget(this);
        }

        protected virtual void OnDestroy()
        {
            EventMgr.Instance.UnSubscribeByTarget(this);
        }
    }

}
