
using System;
using System.Collections.Generic;

namespace Easy
{
    /// <summary>
    /// 定时回调
    /// </summary>
    /// <param name="objs"></param>
    public delegate void TimerCallBack(params object[] objs);

    /// <summary>
    /// 定时器对象
    /// </summary>
    public class TimerObj : IPoolObject, IComparable<TimerObj>
    {
        /// <summary>
        /// 触发事件点
        /// </summary>
        public long tickTime;
        /// <summary>
        /// 优先级
        /// </summary>
        public int priority;
        /// <summary>
        /// 是否循环,循环几次
        /// </summary>
        public int loop;
        /// <summary>
        /// 间隔事件
        /// </summary>
        public long loopInterval;
        /// <summary>
        /// 回调参数
        /// </summary>
        public object[] args;
        /// <summary>
        /// 回调方法
        /// </summary>
        public TimerCallBack callback;
        /// <summary>
        /// 方法对象
        /// </summary>
        public object target;

        public int CompareTo(TimerObj other)
        {
            if (tickTime == other.tickTime)
            {
                return priority - other.priority > 0 ? 1 : -1;
            }
            return tickTime - other.tickTime > 0 ? 1 : -1;
        }

        public void Reset()
        {
            tickTime = 0;
            priority = 0;
            loop = 0;
            loopInterval = 0;
            args = null;
            callback = null;
            target = null;
        }
    }

    /// <summary>
    /// 定时器管理类
    /// </summary>
    public class TimerMgr : Singleton<TimerMgr>
    {
        private List<TimerObj> _timerObjs = new List<TimerObj>();

        private bool _changed = false;

        public override void BeforeRestart()
        {
            Clear();
        }

        public override void Init(InitCompleteCallback complete)
        {
            complete.Invoke(true);
        }

        /// <summary>
        /// 注册定时器
        /// </summary>
        /// <param name="timerObj"></param>
        /// <returns></returns>
        public TimerObj Register(TimerObj timerObj)
        {
            timerObj.tickTime += DateTime.Now.Ticks;
            _timerObjs.Add(timerObj);
            _changed = true;
            return timerObj;
        }

        /// <summary>
        /// 注册定时器
        /// </summary>
        /// <param name="target"></param>
        /// <param name="timerCallBack"></param>
        /// <param name="args"></param>
        /// <param name="priority"></param>
        /// <param name="tickTime"></param>
        /// <param name="loop"></param>
        /// <param name="loopInterval"></param>
        /// <returns></returns>
        public TimerObj Register(object target, TimerCallBack timerCallBack, object[] args , int priority = 1, long tickTime = 0, int loop = 0, long loopInterval = TimeSpan.TicksPerSecond)
        {
            TimerObj timerObj = Get();
            timerObj.target = target;
            timerObj.callback = timerCallBack;
            timerObj.args = args;
            timerObj.tickTime = tickTime;
            timerObj.loop = loop;
            timerObj.loopInterval = loopInterval;
            return Register(timerObj);
        }

        /// <summary>
        /// 注销定时器
        /// </summary>
        /// <param name="timerObj"></param>
        public void UnRegister(TimerObj timerObj)
        {
            for (int i = _timerObjs.Count - 1; i >= 0; --i)
            {
                if (_timerObjs[i] == timerObj)
                {
                    _timerObjs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 通过对象注销定时器
        /// </summary>
        /// <param name="target"></param>
        public void ClearByTarget(object target)
        {
            for (int i = _timerObjs.Count - 1; i >= 0; --i)
            {
                if (_timerObjs[i].target == target)
                {
                    _timerObjs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            long now = DateTime.Now.Ticks;
            if(_changed)
            {
                _timerObjs.Sort();
                _changed = false;
            }
            for (int i = 0; i < _timerObjs.Count; ++i)
            {
                TimerObj timerObj = _timerObjs[i];
                if (timerObj.tickTime <= now)
                {
                    timerObj.callback(timerObj.args);
                    if (timerObj.loop == 0)
                    {
                        _timerObjs.RemoveAt(i);
                        Put(timerObj);
                        --i;
                    }
                    else if (timerObj.loop > 0)
                    {
                        --timerObj.loop;
                        timerObj.tickTime += timerObj.loopInterval;
                        _changed = true;
                    }
                    else
                    {
                        timerObj.tickTime += timerObj.loopInterval;
                        _changed = true;
                    }
                }
            }
        }

        /// <summary>
        /// 清空定时器
        /// </summary>
        public void Clear()
        {
            foreach (var timerObj in _timerObjs)
            {
                Put(timerObj);
            }
            _timerObjs.Clear();
        }

        /// <summary>
        /// 缓存中获得定时器
        /// </summary>
        /// <returns></returns>
        public TimerObj Get()
        {
            return ObjectPoolMgr.Instance.GetObject<TimerObj>();
        }

        /// <summary>
        /// 回收定时器
        /// </summary>
        /// <param name="timerObj"></param>
        public void Put(TimerObj timerObj)
        {
            ObjectPoolMgr.Instance.PutObject(timerObj);
        }
    }

}
