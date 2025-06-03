using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public static class EasyTaskRunner
{
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

    public static async EasyVoidTask Yeild()
    {
        await EasyYeildTask.Create(true);
    }

    public static async EasyVoidTask Yeild(EasyCancellationToken cancellationToken = null)
    {
        await EasyYeildTask.Create(true).SetCancellationToken(cancellationToken);
    }

    public static async EasyVoidTask Delay(float delay, EasyCancellationToken cancellationToken = null)
    {
        await EasyDelayTask.Create(true).SetDelayTime(delay).SetCancellationToken(cancellationToken);
    }

    public static async EasyTask<bool> WaitAll(IEasyTaskInterface[] tasks, EasyCancellationToken cancellationToken = null)
    {
        await EasyWailAllTask.Create(true).SetWaitTasks(tasks).SetCancellationToken(cancellationToken);
        return true;
    }

    public static async EasyTask<bool> WaitAll(IEasyTaskInterface[] tasks)
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
                task.Recycle();
                timingEasyTasks.RemoveAt(i);
                continue;
            }
            if (task.EasyCancellationToken?.IsCanceled == true)
            {
                task.SetException(new EasyCancelException());
                continue;
            }
            if (task.GetTiming() == timing)
            {
                task.MoveNext();
            }
        }
    }
}