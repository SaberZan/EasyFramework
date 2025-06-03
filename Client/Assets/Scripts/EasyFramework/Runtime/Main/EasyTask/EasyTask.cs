using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
    public void Recycle();
}

[AsyncMethodBuilder(typeof(EasyAsyncGenericTaskMethodBuider<>))]
public class EasyTask<T> : IEasyTaskInterface, ICriticalNotifyCompletion
{
    protected static readonly ConcurrentQueue<EasyTask<T>> queue = new ConcurrentQueue<EasyTask<T>>();
    public static EasyTask<T> Create(bool autoRecycle = false)
    {
        EasyTask<T> t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyTask<T>();
        }
        t.autoRecycle = autoRecycle;
        return t;
    }

    public virtual void Recycle()
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

    protected EasyTask()
    {
        EasyTaskRunner.AddTask(this);
    }

    public EasyTask<T> GetAwaiter()
    {
        return this;
    }

    public bool IsCompleted => easyTaskState != EasyTaskState.Pending;

  
    public virtual T GetResult()
    {
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
        EasyVoidTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyVoidTask();
        }
        t.autoRecycle = autoRecycle;
        return t;
    }
}

public class EasyYeildTask : EasyVoidTask
{
    protected new static readonly ConcurrentQueue<EasyYeildTask> queue = new ConcurrentQueue<EasyYeildTask>();
    public new static EasyYeildTask Create(bool autoRecycle = false)
    {
        EasyYeildTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyYeildTask();
        }
        t.autoRecycle = autoRecycle;
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
        EasyDelayTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyDelayTask();
        }
        t.autoRecycle = autoRecycle;
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
        EasyWailAllTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyWailAllTask();
        }
        t.autoRecycle = autoRecycle;
        return t;
    }

    private IEasyTaskInterface[] easyTasks;
    public EasyWailAllTask SetWaitTasks(params IEasyTaskInterface[] tasks)
    {
        easyTasks = tasks;
        return this;
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
        else if(completedNum == easyTasks.Length)
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
        EasyWhenAnyTask t;
        if (!queue.TryDequeue(out t))
        {
            t = new EasyWhenAnyTask();
        }
        t.autoRecycle = autoRecycle;
        return t;
    }

    private IEasyTaskInterface[] easyTasks;

    public EasyWhenAnyTask SetWaitTasks(params IEasyTaskInterface[] tasks)
    {
        easyTasks = tasks;
        return this;
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
        else if(completedNum != 0)
        {
            SetResult(null);
        }
    }
}