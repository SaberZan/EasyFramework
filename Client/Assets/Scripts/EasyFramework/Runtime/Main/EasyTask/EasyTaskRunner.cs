using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Easy
{

    public enum EasyTaskState
    {
        Pending,
        Completed,
        Canceled,
        Faulted
    }

    public enum Timing
    {
        Auto = 0,
        Main = 10,
        Thread = 100,
    }

    public enum ThreadMode
    {
        RunInThreadPool,
        RunInOneThread,
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

    public class EasyCancelException : Exception
    {
        public override string Message { get => "EasyTask Canceled"; }
    }

    public class EasyTaskRunnerThreadException : Exception
    {
        public override string Message { get => "EasyTaskRunner ThreadMode Not Start"; }
    }

    public class EasyTaskPoolException : Exception
    {
        public override string Message { get => "EasyTask Has Been Recycled"; }
    }

    public class EasyCancellationToken
    {
        public bool IsCanceled { get; set; }
        public void Cancel()
        {
            IsCanceled = true;
        }
    }

    public static class EasyTaskRunner
    {
        public delegate void EasyTaskLog(string message);
        public static EasyTaskLog Log = (message) => { };
        public static bool IsStartThreadTiming = false;
        public static Thread thread;
        private static ConcurrentQueue<(Action, EasyCancellationToken)> threadAction = new ConcurrentQueue<(Action, EasyCancellationToken)>();
        private static Dictionary<Timing, List<IEasyTaskInterface>> easyTasks = new Dictionary<Timing, List<IEasyTaskInterface>>();
        private static Dictionary<Timing, int> easyTasksCountRecord = new Dictionary<Timing, int>();
        private static ConcurrentDictionary<Timing, ConcurrentBag<IEasyTaskInterface>> easyTasksAddTmp = new ConcurrentDictionary<Timing, ConcurrentBag<IEasyTaskInterface>>();
        public static void AddTask(IEasyTaskInterface task)
        {
            while (true)
            {
                if (!easyTasksAddTmp.ContainsKey(task.GetTiming()))
                {
                    easyTasksAddTmp.TryAdd(task.GetTiming(), new ConcurrentBag<IEasyTaskInterface>());
                }
                if (easyTasksAddTmp.TryGetValue(task.GetTiming(), out var list))
                {
                    list.Add(task);
                    break;
                }
            }
        }

        public static void ResetTask()
        {
            IsStartThreadTiming = false;
            thread?.Abort();
            easyTasksAddTmp.Clear();
            easyTasks.Clear();
            threadAction.Clear();
        }

        public static async void StartThreadTiming(ThreadMode threadMode = ThreadMode.RunInThreadPool, int tickInterval = 15)
        {
            IsStartThreadTiming = true;
            if (threadMode == ThreadMode.RunInThreadPool)
            {
                int isRun = 0;
                while (IsStartThreadTiming)
                {
                    await Delay(tickInterval);
                    if (isRun == 0)
                    {
                        ThreadPool.QueueUserWorkItem((obj) =>
                        {
                            Interlocked.Increment(ref isRun);
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
                            Interlocked.Decrement(ref isRun);
                        });
                    }
                }
            }
            else if (threadMode == ThreadMode.RunInOneThread)
            {
                thread = new Thread(() =>
                {
                    while (IsStartThreadTiming)
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
                        Thread.Sleep(tickInterval);
                    }
                });
                thread.Start();
            }
        }

        public static EasyYieldTask Yield(EasyCancellationToken cancellationToken = null)
        {
            return (EasyYieldTask)EasyYieldTask.Create(true).SetCancellationToken(cancellationToken);
        }

        public static EasyVoidTask Delay(int milli, EasyCancellationToken cancellationToken = null)
        {
            return (EasyDelayTask)EasyDelayTask.Create(true).SetDelayTime(milli).SetCancellationToken(cancellationToken);
        }

        public static EasyWailAllTask WaitAll(IEasyTaskInterface[] tasks, EasyCancellationToken cancellationToken = null)
        {
            return (EasyWailAllTask)EasyWailAllTask.Create(true).SetWaitTasks(tasks).SetCancellationToken(cancellationToken);
        }

        public static EasyWailAllTask WaitAll(params IEasyTaskInterface[] tasks)
        {
            return EasyWailAllTask.Create(true).SetWaitTasks(tasks);
        }

        public static EasyWaitAnyTask WaitAny(IEasyTaskInterface[] tasks, EasyCancellationToken cancellationToken = null)
        {
            return (EasyWaitAnyTask)EasyWaitAnyTask.Create(true).SetWaitTasks(tasks).SetCancellationToken(cancellationToken);
        }

        public static EasyWaitAnyTask WaitAny(params IEasyTaskInterface[] tasks)
        {
            return EasyWaitAnyTask.Create(true).SetWaitTasks(tasks);
        }

        public static EasyWaitUntilTask WaitUntil(Func<bool> condition)
        {
            return EasyWaitUntilTask.Create(true).SetCondition(condition);
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
                throw new EasyTaskRunnerThreadException();
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

            int preTaskCount = 0;
            if (!easyTasksCountRecord.TryGetValue(timing, out preTaskCount))
            {
                easyTasksCountRecord.TryAdd(timing, preTaskCount);
            }
            int nowCount = timingEasyTasks.Count;
            if (preTaskCount != nowCount)
            {
                easyTasksCountRecord[timing] = nowCount;
                Debug.Log($"{timing}  timingEasyTasks count  {nowCount}");
            }
        }
    }
}