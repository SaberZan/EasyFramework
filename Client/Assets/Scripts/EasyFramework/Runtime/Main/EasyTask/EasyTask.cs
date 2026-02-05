using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Easy
{
    [AsyncMethodBuilder(typeof(EasyAsyncGenericTaskMethodBuider<>))]
    public class EasyTask<T> : IEasyTaskInterface, ICriticalNotifyCompletion
    {
        protected static readonly ConcurrentQueue<EasyTask<T>> queue = new ConcurrentQueue<EasyTask<T>>();

        public static Timing CheckTiming(Timing timing)
        {
            if (timing == Timing.Auto)
            {
                timing = EasyTaskRunner.IsStartThreadTiming && Thread.CurrentThread.ManagedThreadId != 1 ? Timing.Thread : Timing.Main;
            }
            else if(timing == Timing.Thread && !EasyTaskRunner.IsStartThreadTiming)
            {
                throw new EasyTaskRunnerThreadException();
            }
            return timing;
        } 
        public static EasyTask<T> Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyTask<T> t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyTask<T>();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        private EasyTaskState easyTaskState;

        public EasyTaskState EasyTaskState
        {
            get
            {
                return easyTaskState;
            }
            set
            {
                easyTaskState = value;
                if (IsCompleted)
                {
                    callback?.Invoke();
                }
            }
        }

        private EasyCancellationToken cancellationToken;
        public EasyCancellationToken EasyCancellationToken => cancellationToken;
        public Action callback;
        protected T result;
        protected bool autoRecycle = false;
        protected Timing timing = Timing.Main;
        protected int reference = 0;
        protected bool isInPool = false; 
        protected bool getResultCalled = false;

        protected EasyTask()
        {

        }

        public virtual void Reuse(Timing timing, bool autoRecycle)
        {
            this.timing = CheckTiming(timing);
            this.autoRecycle = autoRecycle;
            isInPool = false;
            getResultCalled = false;
            EasyTaskRunner.AddTask(this);
            Retain();
        }

        public virtual void Unuse()
        {
            if (!autoRecycle)
            {
                return;
            }
            isInPool = true;
            getResultCalled = false;
            autoRecycle = false;
            this.callback = null;
            result = default;
            easyTaskState = EasyTaskState.Pending;
            cancellationToken = null;
            queue.Enqueue(this);
        }

        public void Retain()
        {
            Interlocked.Increment(ref reference);
        }

        public void Release()
        {
            Interlocked.Decrement(ref reference);
        }

        public bool IsFree()
        {
            return reference <= 0;
        }

        public EasyTask<T> GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted => easyTaskState != EasyTaskState.Pending;

    
        public virtual T GetResult()
        {
            if (isInPool)
            {
                throw new EasyTaskPoolException();
            }
            if (!getResultCalled)
            {
                getResultCalled = true;
                Release();
            }
            T tmp = result;
            return tmp;
        }

        public void SetResult(T t)
        {
            result = t;
            EasyTaskState = EasyTaskState.Completed;
        }

        public void SetException(Exception e)
        {
            if (e is EasyCancelException)
            {
                EasyTaskState = EasyTaskState.Canceled;
            }
            else
            {
                EasyTaskState = EasyTaskState.Faulted;
            }
            EasyTaskRunner.Log?.Invoke(e.ToString());
        }

        public EasyTask<T> SetCancellationToken(EasyCancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            return this;
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation?.Invoke();
                return;
            }
            callback = continuation;
        }

        public Timing GetTiming()
        {
            return timing;
        }

        public virtual void MoveNext()
        {

        }
    }

    [AsyncMethodBuilder(typeof(EasyAsyncGenericTaskMethodBuider))]
    public class EasyVoidTask : EasyTask<object>
    {
        protected new static readonly ConcurrentQueue<EasyVoidTask> queue = new ConcurrentQueue<EasyVoidTask>();
        public new static EasyVoidTask Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyVoidTask t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyVoidTask();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        protected EasyVoidTask()
        {

        }
    }

    public class EasyYieldTask : EasyVoidTask
    {
        protected new static readonly ConcurrentQueue<EasyYieldTask> queue = new ConcurrentQueue<EasyYieldTask>();
        public new static EasyYieldTask Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyYieldTask t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyYieldTask();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        protected EasyYieldTask()
        {

        }

        public override void MoveNext()
        {
            SetResult(null);
        }
    }

    public class EasyExecFunTask : EasyVoidTask
    {
        protected new static readonly ConcurrentQueue<EasyExecFunTask> queue = new ConcurrentQueue<EasyExecFunTask>();
        public new static EasyExecFunTask Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyExecFunTask t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyExecFunTask();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        private Action func;

        protected EasyExecFunTask()
        {

        }

        public override void Unuse()
        {
            func = null;
            base.Unuse();
        }

        public EasyExecFunTask SetFunc(Action func)
        {
            this.func = func;
            return this;
        }

        public override void MoveNext()
        {
            func?.Invoke();
            SetResult(null);       
        }
    }

    public class EasyRunTask<T> : EasyTask<T>
    {
        protected new static readonly ConcurrentQueue<EasyRunTask<T>> queue = new ConcurrentQueue<EasyRunTask<T>>();
        public new static EasyRunTask<T> Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyRunTask<T> t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyRunTask<T>();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        private Func<T> func;
        private bool isRunning;

        protected EasyRunTask()
        {

        }

        public override void Unuse()
        {
            func = null;
            isRunning = false;
            base.Unuse();
        }


        public EasyRunTask<T> SetFunc(Func<T> func)
        {
            this.func = func;
            return this;
        }

        public override void MoveNext()
        {
            if (isRunning)
                return;
            if (ThreadPool.QueueUserWorkItem(Run))
            {
                isRunning = true;
            }
        }

        private void Run(object result)
        {
            if (func == null)
            {
                SetResult(default);
            }
            else
            {
                SetResult(func());
            }

        }
    }

    public class EasyRunTask : EasyVoidTask
    {
        protected new static readonly ConcurrentQueue<EasyRunTask> queue = new ConcurrentQueue<EasyRunTask>();
        public new static EasyRunTask Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyRunTask t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyRunTask();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        private Action func;
        private bool isRunning;

        protected EasyRunTask()
        {

        }

        public override void Unuse()
        {
            func = null;
            isRunning = false;
            base.Unuse();
        }

        public EasyRunTask SetFunc(Action func)
        {
            this.func = func;
            return this;
        }

        public override void MoveNext()
        {
            if (isRunning)
                return;
            if (ThreadPool.QueueUserWorkItem(Run))
            {
                isRunning = true;
            }
        }

        private void Run(object result)
        {
            func.Invoke();
            SetResult(null);
        }
    }

    public class EasyDelayTask : EasyVoidTask
    {
        protected new static readonly ConcurrentQueue<EasyDelayTask> queue = new ConcurrentQueue<EasyDelayTask>();
        public new static EasyDelayTask Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyDelayTask t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyDelayTask();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        private long overTime;

        protected EasyDelayTask()
        {

        }

        public EasyDelayTask SetDelayTime(int milli)
        {
            overTime = DateTime.UtcNow.Ticks + TimeSpan.TicksPerMillisecond * milli;
            return this;
        }
        public override void MoveNext()
        {
            if (IsCompleted)
            {
                return;
            }
            if (DateTime.UtcNow.Ticks >= overTime)
            {
                SetResult(null);
            }
        }
    }

    public class EasyWaitUntilTask : EasyVoidTask
    {
        protected new static readonly ConcurrentQueue<EasyWaitUntilTask> queue = new ConcurrentQueue<EasyWaitUntilTask>();
        public new static EasyWaitUntilTask Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyWaitUntilTask t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyWaitUntilTask();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        private Func<bool> condition;

        protected EasyWaitUntilTask()
        {

        }

        public override void Unuse()
        {
            condition = null;
            base.Unuse();
        }

        public EasyWaitUntilTask SetCondition(Func<bool> condition)
        {
            this.condition = condition;
            return this;
        }
        public override void MoveNext()
        {
            if (IsCompleted)
            {
                return;
            }
            if (condition == null)
            {
                SetResult(null);
                return;
            }
            if (condition())
            {
                SetResult(null);
            }
        }
    }

    public class EasyWailAllTask : EasyVoidTask
    {
        protected new static readonly ConcurrentQueue<EasyWailAllTask> queue = new ConcurrentQueue<EasyWailAllTask>();
        public new static EasyWailAllTask Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyWailAllTask t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyWailAllTask();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        private IEasyTaskInterface[] easyTasks;

        protected EasyWailAllTask()
        {

        }

        public override void Unuse()
        {
            easyTasks = null;
            base.Unuse();
        }

        public EasyWailAllTask SetWaitTasks(params IEasyTaskInterface[] tasks)
        {
            easyTasks = tasks;
            for (int i = 0; i < easyTasks.Length; i++)
            {
                easyTasks[i].Retain();
            }
            return this;
        }

        public override object GetResult()
        {
            if (isInPool)
            {
                throw new EasyTaskPoolException();
            }
            if (!getResultCalled)
            {
                getResultCalled = true;
                Release();
                for (int i = 0; i < easyTasks.Length; i++)
                {
                    easyTasks[i].Release();
                }
            }
            object tmp = result;
            return tmp;
        }

        public override void MoveNext()
        {
            if (IsCompleted)
            {
                return;
            }
            int completedNum = 0;
            int faultedNum = 0;
            int cancelNum = 0;
            for (int i = easyTasks.Length - 1; i >= 0; --i)
            {
                if (easyTasks[i].EasyTaskState == EasyTaskState.Completed)
                {
                    completedNum++;
                }
                else if (easyTasks[i].EasyTaskState == EasyTaskState.Faulted)
                {
                    faultedNum++;
                }
                else if (easyTasks[i].EasyTaskState == EasyTaskState.Canceled)
                {
                    cancelNum++;
                }
            }
            if (faultedNum != 0)
            {
                SetException(new Exception("Has Task Faulted"));
            }
            else if (cancelNum != 0)
            {
                SetException(new Exception("Has Task Canceled"));
            }
            else if (completedNum == easyTasks.Length)
            {
                SetResult(null);
            }
        }
    }

    public class EasyWaitAnyTask : EasyTask<int>
    {
        protected new static readonly ConcurrentQueue<EasyWaitAnyTask> queue = new ConcurrentQueue<EasyWaitAnyTask>();
        public new static EasyWaitAnyTask Create(bool autoRecycle = false, Timing timing = Timing.Auto)
        {
            EasyWaitAnyTask t;
            if (!queue.TryDequeue(out t))
            {
                t = new EasyWaitAnyTask();
            }
            t.Reuse(timing, autoRecycle);
            return t;
        }

        private IEasyTaskInterface[] easyTasks;

        protected EasyWaitAnyTask()
        {

        }

        public override void Unuse()
        {
            easyTasks = null;
            base.Unuse();
        }

        public EasyWaitAnyTask SetWaitTasks(params IEasyTaskInterface[] tasks)
        {
            easyTasks = tasks;
            for (int i = 0; i < easyTasks.Length; i++)
            {
                easyTasks[i].Retain();
            }
            return this;
        }

        public override int GetResult()
        {
            if (isInPool)
            {
                throw new EasyTaskPoolException();
            }
            if (!getResultCalled)
            {
                getResultCalled = true;
                Release();
                for (int i = 0; i < easyTasks.Length; i++)
                {
                    easyTasks[i].Release();
                }
            }
            int tmp = result;
            return tmp;
        }

        public override void MoveNext()
        {
            if (IsCompleted)
            {
                return;
            }
            int completedIndex = -1;
            int faultedNum = 0;
            int cancelNum = 0;
            for (int i = easyTasks.Length - 1; i >= 0; --i)
            {
                if (easyTasks[i].EasyTaskState == EasyTaskState.Completed)
                {
                    completedIndex = i;
                    break;
                }
                else if (easyTasks[i].EasyTaskState == EasyTaskState.Faulted)
                {
                    faultedNum++;
                }
                else if (easyTasks[i].EasyTaskState == EasyTaskState.Canceled)
                {
                    cancelNum++;
                }
            }
            if (faultedNum + cancelNum == easyTasks.Length)
            {
                SetException(new Exception("All Task Faulted Or Canceled"));
            }
            else if (completedIndex != -1)
            {
                SetResult(completedIndex);
            }
        }
    }
}