using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

public enum EasyTaskState
{
    Pending,
    Completed,
    Canceled,
    Faulted
}

public enum Timing
{   
    Main = 10,
    Thread = 100,
}

public interface IEasyTaskInterface
{
    public Timing GetTiming();
    public void MoveNext();
    public bool IsCompleted { get; }
    public EasyTaskState EasyTaskState { get; }
    public EasyCancellationToken EasyCancellationToken { get; }
    public void SetException(Exception e);
    public void Reuse(Timing timing, bool autoRecycle);
    public void Unuse();
    public void Retain();
    public void Release();
    public bool IsFree();
}

[AsyncMethodBuilder(typeof(EasyAsyncGenericTaskMethodBuider<>))]
public class EasyTask<T> : IEasyTaskInterface, ICriticalNotifyCompletion
{
    protected static readonly ConcurrentQueue<EasyTask<T>> queue = new ConcurrentQueue<EasyTask<T>>();
    public static EasyTask<T> Create(bool autoRecycle = false)
    {

        Timing timing = EasyTaskRunner.IsStartThreadTiming && Thread.CurrentThread.ManagedThreadId != 1 ? Timing.Thread : Timing.Main;
        EasyTask<T> t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyTask<T>();
        }
        t.Reuse(timing, autoRecycle);
        return t;
    }

    public virtual void Reuse(Timing timing, bool autoRecycle)
    {
        this.timing = timing;
        this.autoRecycle = autoRecycle;
        EasyTaskRunner.AddTask(this);
        Retain();
    }

    public virtual void Unuse()
    {
        if (!autoRecycle)
        {
            return;
        }
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
            if (easyTaskState == EasyTaskState.Completed)
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

    protected EasyTask()
    {
        
    }

    public EasyTask<T> GetAwaiter()
    {
        return this;
    }

    public bool IsCompleted => easyTaskState != EasyTaskState.Pending;

  
    public virtual T GetResult()
    {
        Release();
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

    public EasyTask<T> SetTiming(Timing timing)
    {
        this.timing = timing;
        return this;
    }

    public virtual void MoveNext()
    {

    }

}

[AsyncMethodBuilder(typeof(EasyAsyncGenericTaskMethodBuider))]
public class EasyVoidTask : EasyTask<object>
{
    protected new static readonly ConcurrentQueue<EasyVoidTask> queue = new ConcurrentQueue<EasyVoidTask>();
    public new static EasyVoidTask Create(bool autoRecycle = false)
    {
        Timing timing = EasyTaskRunner.IsStartThreadTiming && Thread.CurrentThread.ManagedThreadId != 1 ? Timing.Thread : Timing.Main;
        EasyVoidTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyVoidTask();
        }
        t.Reuse(timing, autoRecycle);
        return t;
    }
}

public class EasyYieldTask : EasyVoidTask
{
    protected new static readonly ConcurrentQueue<EasyYieldTask> queue = new ConcurrentQueue<EasyYieldTask>();
    public new static EasyYieldTask Create(bool autoRecycle = false)
    {
        Timing timing = EasyTaskRunner.IsStartThreadTiming && Thread.CurrentThread.ManagedThreadId != 1 ? Timing.Thread : Timing.Main;
        EasyYieldTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyYieldTask();
        }
        t.Reuse(timing, autoRecycle);
        return t;
    }

    public override void MoveNext()
    {
        SetResult(null);
    }
}

public class EasyDelayTask : EasyVoidTask
{
    protected new static readonly ConcurrentQueue<EasyDelayTask> queue = new ConcurrentQueue<EasyDelayTask>();
    public new static EasyDelayTask Create(bool autoRecycle = false)
    {
        Timing timing = EasyTaskRunner.IsStartThreadTiming && Thread.CurrentThread.ManagedThreadId != 1 ? Timing.Thread : Timing.Main;
        EasyDelayTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyDelayTask();
        }
        t.Reuse(timing, autoRecycle);
        return t;
    }

    private long overTime;
    public EasyDelayTask SetDelayTime(float delayTime)
    {
        overTime = DateTime.UtcNow.Ticks + (long)(TimeSpan.TicksPerSecond * delayTime);
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

public class EasyWailAllTask : EasyVoidTask
{
    protected new static readonly ConcurrentQueue<EasyWailAllTask> queue = new ConcurrentQueue<EasyWailAllTask>();
    public new static EasyWailAllTask Create(bool autoRecycle = false)
    {
        Timing timing = EasyTaskRunner.IsStartThreadTiming && Thread.CurrentThread.ManagedThreadId != 1 ? Timing.Thread : Timing.Main;
        EasyWailAllTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyWailAllTask();
        }
        t.Reuse(timing, autoRecycle);
        return t;
    }

    public override void Unuse()
    {
        easyTasks = null;
        base.Unuse();
    }

    private IEasyTaskInterface[] easyTasks;
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
        Release();
        for (int i = 0; i < easyTasks.Length; i++)
        {
            easyTasks[i].Release();
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
        }
        if (faultedNum != 0)
        {
            SetException(new Exception("Has task faulted"));
        }
        else if (completedNum == easyTasks.Length)
        {
            SetResult(null);
        }
    }
}

[AsyncMethodBuilder(typeof(EasyAsyncGenericTaskMethodBuider))]
public class EasyWhenAnyTask : EasyVoidTask
{
    protected new static readonly ConcurrentQueue<EasyWhenAnyTask> queue = new ConcurrentQueue<EasyWhenAnyTask>();
    public new static EasyWhenAnyTask Create(bool autoRecycle = false)
    {
        Timing timing = EasyTaskRunner.IsStartThreadTiming && Thread.CurrentThread.ManagedThreadId != 1 ? Timing.Thread : Timing.Main;
        EasyWhenAnyTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyWhenAnyTask();
        }
        t.Reuse(timing, autoRecycle);
        return t;
    }

    public override void Unuse()
    {
        easyTasks = null;
        base.Unuse();
    }

    private IEasyTaskInterface[] easyTasks;

    public EasyWhenAnyTask SetWaitTasks(params IEasyTaskInterface[] tasks)
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
        Release();
        for (int i = 0; i < easyTasks.Length; i++)
        {
            easyTasks[i].Release();
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
        for (int i = easyTasks.Length - 1; i >= 0; --i)
        {
            if (easyTasks[i].EasyTaskState == EasyTaskState.Completed)
            {
                completedNum++;
                break;
            }
            else if (easyTasks[i].EasyTaskState == EasyTaskState.Faulted)
            {
                faultedNum++;
            }
        }
        if (faultedNum == easyTasks.Length)
        {
            SetException(new Exception("All task faulted"));
        }
        else if (completedNum != 0)
        {
            SetResult(null);
        }
    }
}