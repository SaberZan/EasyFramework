using System.Linq.Expressions;

namespace Easy
{

    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public delegate void EventHandle(EventArg eventArg);

    [AttributeUsage(AttributeTargets.Method)]
    public class BindEvent : Attribute
    {
        public string[] evenKeys;

        public BindEvent(string[] keys)
        {
            evenKeys = keys;
        }
        
        public BindEvent(string key)
        {
            evenKeys = new[] { key };
        }
    }

    public class EventArg : IPoolObject
    {
        public bool isUsed = false;

        public virtual void Reset()
        {
            isUsed = false;
        }
    }

    public class EventData
    {
        public string eventKey;

        public EventArg eventArg;
    }

    public class EventObj
    {
        public object eventTarget;

        // public EventHandle eventHandle;
        public MethodInfo eventHandle;
    }

    [Update]
    [PreInit]
    [OrderIndex((int) PreInitOrderIndexEnum.EventMgr)]
    public class EventMgr : Singleton<EventMgr>
    {
        /// <summary>
        /// 消息队列1
        /// </summary>
        private Queue<EventData> _eventDataQueueFrist = new Queue<EventData>();

        /// <summary>
        /// 消息队列2
        /// </summary>
        private Queue<EventData> _eventDataQueueSecond = new Queue<EventData>();

        /// <summary>
        /// 事件对象集合
        /// </summary>
        private readonly Dictionary<string, List<EventObj>> _eventDictionary = new Dictionary<string, List<EventObj>>();

        /// <summary>
        /// 重启接口
        /// </summary>
        public override void BeforeRestart()
        {
            Clear();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="complete"></param>
        public override void Init(InitCompleteCallback complete)
        {
            complete.Invoke(true);
        }

        /// <summary>
        /// 注册监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="target"></param>
        /// <param name="handle"></param>
        public void Subscribe(string key, object target, EventHandle handle)
        {
            Subscribe(key, target, handle.GetMethodInfo());
        }
        
        /// <summary>
        /// 注销监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="target"></param>
        /// <param name="handle"></param>
        public void UnSubscribe(string key, object target, EventHandle handle)
        {
            UnSubscribe(key, target, handle.GetMethodInfo());
        }
        
        /// <summary>
        /// 注册监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="target"></param>
        /// <param name="handle"></param>
        public void Subscribe(string key, object target, MethodInfo handle)
        {
            if (!_eventDictionary.ContainsKey(key))
            {
                _eventDictionary.Add(key, new List<EventObj>());
            }

            _eventDictionary[key].Add(new EventObj() {eventTarget = target, eventHandle = handle});
        }

        /// <summary>
        /// 注销监听
        /// </summary>
        /// <param name="key"></param>
        /// <param name="target"></param>
        /// <param name="handle"></param>
        public void UnSubscribe(string key, object target, MethodInfo handle)
        {
            if (_eventDictionary.ContainsKey(key))
            {
                List<EventObj> eventList = _eventDictionary[key];
                for (int i = eventList.Count - 1; i >= 0; --i)
                {
                    if (eventList[i].eventTarget == target && handle == eventList[i].eventHandle)
                    {
                        eventList.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 注册监听
        /// </summary>
        /// <param name="target"></param>
        public void SubscribeByTarget(object target)
        {
            var type = target.GetType();
            MethodInfo[] allMethodInfos =
                type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MethodInfo method in allMethodInfos)
            {
                if (method.IsDefined(typeof(BindEvent)))
                {
                    string[] evenKeys = method.GetCustomAttribute<BindEvent>().evenKeys;
                    // ParameterExpression param = Expression.Parameter(typeof(EventArg));
                    // MethodCallExpression methodCall = Expression.Call(Expression.Constant(target), method, param);
                    // EventHandle handle = Expression.Lambda<EventHandle>(methodCall,new ParameterExpression[]{param}).Compile();
                    // foreach (var evenKey in evenKeys)
                    // {
                    //     Subscribe(evenKey, target, handle);
                    // }
                    foreach (var evenKey in evenKeys)
                    {
                        Subscribe(evenKey, target,  method);
                    }
                }
            }
        }

        /// <summary>
        /// 注销监听
        /// </summary>
        /// <param name="target"></param>
        public void UnSubscribeByTarget(object target)
        {
            foreach (KeyValuePair<string, List<EventObj>> kv in _eventDictionary)
            {
                List<EventObj> eventList = kv.Value;
                for (int i = eventList.Count - 1; i >= 0; --i)
                {
                    if (target == eventList[i].eventTarget)
                    {
                        eventList.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 派发事件定时更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            var tmp = _eventDataQueueFrist;
            _eventDataQueueFrist = _eventDataQueueSecond;
            _eventDataQueueSecond = tmp;
            while (_eventDataQueueFrist.Count > 0)
            {
                EventData data = _eventDataQueueFrist.Dequeue();
                InternalDispatchEvent(data.eventKey, data.eventArg);
            }
        }

        /// <summary>
        /// 派发事件,线程安全,下一帧派发
        /// </summary>
        /// <param name="key"></param>
        /// <param name="arg"></param>
        public void DispatchEvent(string key, EventArg arg = null)
        {
            lock(_eventDataQueueSecond)
            {
                if(arg != null)
                {
                    if(arg.isUsed)
                    {
                        throw new ArgumentException("事件参数已使用");
                    }
                    arg.isUsed = true;
                }
                _eventDataQueueSecond.Enqueue(new EventData() {eventKey = key, eventArg = arg});
            }
        }

        /// <summary>
        /// 立即派发事件,非线程安全
        /// </summary>
        /// <param name="key"></param>
        /// <param name="arg"></param>
        public void DispatchEventNow(string key, EventArg arg = null)
        {
            if (arg != null)
            {
                if (arg.isUsed)
                {
                    throw new ArgumentException("事件参数已使用");
                }
                arg.isUsed = true;
            }

            InternalDispatchEvent(key, arg);
        }

        /// <summary>
        /// 内部派发事件接口
        /// </summary>
        /// <param name="key"></param>
        /// <param name="arg"></param>
        private void InternalDispatchEvent(string key, EventArg arg = null)
        {
            if (_eventDictionary.ContainsKey(key))
            {
                List<EventObj> eventList = _eventDictionary[key];
                for (int i = 0; i < eventList.Count; ++i)
                {
                    eventList[i].eventHandle.Invoke(eventList[i].eventTarget, new object[] {arg});
                }
            }

            if (arg != null)
            {
                PutEventArg(arg);
            }
        }

        /// <summary>
        /// 清空事件
        /// </summary>
        private void Clear()
        {
            while (_eventDataQueueFrist.Count > 0)
            {
                EventArg arg = _eventDataQueueFrist.Peek().eventArg;
                PutEventArg(arg);
            }

            while (_eventDataQueueSecond.Count > 0)
            {
                EventArg arg = _eventDataQueueSecond.Peek().eventArg;
                PutEventArg(arg);
            }

            _eventDictionary.Clear();
        }

        /// <summary>
        /// 从缓存中获取事件参数对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetEventArg<T>() where T : EventArg, new()
        {
            return ObjectPoolMgr.Instance.GetObject<T>();
        }

        /// <summary>
        /// 回收参数对象
        /// </summary>
        /// <param name="eventArg"></param>
        private void PutEventArg(EventArg eventArg)
        {
            ObjectPoolMgr.Instance.PutObject(eventArg);
        }

    }

}