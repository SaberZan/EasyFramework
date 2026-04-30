
using System;
using System.Collections.Generic;

namespace Easy
{
    /// <summary>
    /// 定时回调
    /// </summary>
    /// <param name="objs"></param>
    public delegate void SchedulerCallBack(params object[] objs);

    /// <summary>
    /// 定时器对象
    /// </summary>
    public class SchedulerObj : IPoolObject, IComparable<SchedulerObj>
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
        public SchedulerCallBack callback;
        /// <summary>
        /// 方法对象
        /// </summary>
        public object target;

        public int CompareTo(SchedulerObj other)
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
    public class SchedulerMgr : Singleton<SchedulerMgr>
    {
        private List<SchedulerObj> _timerObjs = new List<SchedulerObj>();

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
        public SchedulerObj Register(SchedulerObj timerObj)
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
        /// <param name="schedulerCallBack"></param>
        /// <param name="args"></param>
        /// <param name="priority"></param>
        /// <param name="tickTime"></param>
        /// <param name="loop"></param>
        /// <param name="loopInterval"></param>
        /// <returns></returns>
        public SchedulerObj Register(object target, SchedulerCallBack schedulerCallBack, object[] args , int priority = 1, long tickTime = 0, int loop = 0, long loopInterval = TimeSpan.TicksPerSecond)
        {
            SchedulerObj timerObj = Get();
            timerObj.target = target;
            timerObj.callback = schedulerCallBack;
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
        public void UnRegister(SchedulerObj timerObj)
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
                SchedulerObj timerObj = _timerObjs[i];
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
        public SchedulerObj Get()
        {
            return ObjectPoolMgr.Instance.GetObject<SchedulerObj>();
        }

        /// <summary>
        /// 回收定时器
        /// </summary>
        /// <param name="timerObj"></param>
        public void Put(SchedulerObj timerObj)
        {
            ObjectPoolMgr.Instance.PutObject(timerObj);
        }
    }

}
