using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public static class EasyTaskRunner
{
    public delegate void EasyTaskLog(string message);
    public static EasyTaskLog Log = (message) => { };
    public static bool IsStartThreadTiming = false;
    public static Thread thread;
    private static ConcurrentQueue<(Action, EasyCancellationToken)> threadAction = new ConcurrentQueue<(Action, EasyCancellationToken)>();
    private static Dictionary<Timing, List<IEasyTaskInterface>> easyTasks = new Dictionary<Timing, List<IEasyTaskInterface>>();
    private static ConcurrentDictionary<Timing, ConcurrentBag<IEasyTaskInterface>> easyTasksAddTmp = new ConcurrentDictionary<Timing, ConcurrentBag<IEasyTaskInterface>>();

    public static void AddTask(IEasyTaskInterface task)
    {
        if (!easyTasksAddTmp.ContainsKey(task.GetTiming()))
        {
            easyTasksAddTmp.TryAdd(task.GetTiming(), new ConcurrentBag<IEasyTaskInterface>());
        }
        easyTasksAddTmp[task.GetTiming()].Add(task);
    }

    public static void ResetTask()
    {
        IsStartThreadTiming = false;
        thread?.Abort();
        easyTasksAddTmp.Clear();
        easyTasks.Clear();
        threadAction.Clear();
    }

    public static async void StartThreadTiming(int tickInterval = 15)
    {
        IsStartThreadTiming = true;


        while (IsStartThreadTiming)
        {
            await Delay(tickInterval);
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                while (threadAction.Count > 0)
                {
                    if (threadAction.TryDequeue(out var action))
                    {
                        if (action.Item2 != null && action.Item2.IsCanceled)
                        {
                            continue;
                        }
                        action.Item1();
                    }
                }
                Tick(Timing.Thread);
            });
        }

        // thread = new Thread(() =>
        // {
        //     while (true)
        //     {
        //         while (threadAction.Count > 0)
        //         {
        //             if (threadAction.TryDequeue(out var action))
        //             {
        //                 if (action.Item2 != null && action.Item2.IsCanceled)
        //                 {
        //                     continue;
        //                 }
        //                 action.Item1();
        //             }
        //         }

        //         Tick(Timing.Thread);
        //         Thread.Sleep(tickInterval);
        //     }
        // });
        // thread.Start();
    }

    public static async EasyVoidTask Yield(EasyCancellationToken cancellationToken = null)
    {
        await EasyYieldTask.Create(true).SetCancellationToken(cancellationToken);
    }

    public static async EasyVoidTask Delay(int milli, EasyCancellationToken cancellationToken = null)
    {
        await EasyDelayTask.Create(true).SetDelayTime(milli).SetCancellationToken(cancellationToken);
    }

    public static async EasyTask<bool> WaitAll(IEasyTaskInterface[] tasks, EasyCancellationToken cancellationToken = null)
    {
        await EasyWailAllTask.Create(true).SetWaitTasks(tasks).SetCancellationToken(cancellationToken);
        return true;
    }

    public static async EasyTask<bool> WaitAll(params IEasyTaskInterface[] tasks)
    {
        await EasyWailAllTask.Create(true).SetWaitTasks(tasks);
        return true;
    }

    public static async EasyTask<bool> WhenAny(IEasyTaskInterface[] tasks, EasyCancellationToken cancellationToken = null)
    {
        await EasyWhenAnyTask.Create(true).SetWaitTasks(tasks).SetCancellationToken(cancellationToken);
        return true;
    }
    
    public static async EasyTask<bool> WhenAny(params IEasyTaskInterface[] tasks)
    {
        await EasyWhenAnyTask.Create(true).SetWaitTasks(tasks);
        return true;
    }

    public static EasyRunTask<T> Run<T>(Func<T> action, EasyCancellationToken cancellationToken = null)
    {
        return (EasyRunTask<T>)EasyRunTask<T>.Create(true).SetFunc(action).SetCancellationToken(cancellationToken);
    }

    public static EasyRunTask Run(Action action, EasyCancellationToken cancellationToken = null)
    {
        return (EasyRunTask)EasyRunTask.Create(true).SetFunc(action).SetCancellationToken(cancellationToken);
    }

    public static void ExecInThread(Action action, EasyCancellationToken cancellationToken = null)
    {
        if (!IsStartThreadTiming)
        {
            throw new Exception("线程任务未开启");
        }
        threadAction.Enqueue((action, cancellationToken));
    }

    public static void Tick(Timing timing)
    {
        if (!easyTasksAddTmp.TryGetValue(timing, out var addTmp))
        {
            return;
        }
        if (!easyTasks.ContainsKey(timing))
        {
            easyTasks.TryAdd(timing, new List<IEasyTaskInterface>());
        }
        easyTasks[timing].AddRange(addTmp);
        addTmp.Clear();

        var timingEasyTasks = easyTasks[timing];
        for (int i = timingEasyTasks.Count - 1; i >= 0; --i)
        {
            var task = timingEasyTasks[i];
            if (task.IsCompleted)
            {
                if (task.IsFree())
                {
                    task.Unuse();
                    timingEasyTasks.RemoveAt(i);
                }
                continue;
            }
            if (task.EasyCancellationToken?.IsCanceled == true)
            {
                task.SetException(new EasyCancelException());
                continue;
            }
            task.MoveNext();
        }
    }
}