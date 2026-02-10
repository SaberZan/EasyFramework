using System.Threading;
using System.Threading.Tasks;
using Easy;
using UnityEngine;

public class EasyTaskTest : MonoBehaviour
{
    private EasyCancellationToken token;

    private CancellationTokenSource cancellationTokenSource;

    private CancellationTokenSource cancellationTokenSource1;

    [EditorButton("Test")]
    public int a = 0;

    [EditorButton("RunTest")]
    public int a1 = 0;

    [EditorButton("TestThread")]
    public int b = 0;

    [EditorButton("Cancel")]
    public int c = 0;

    public void Awake()
    {
        EasyTaskRunner.Log += Debug.LogError;

        // EasyTaskRunner.StartThreadTiming();



    }

    // [Sirenix.OdinInspector.Button]
    public void Test()
    {
        token = new EasyCancellationToken();
        // EasyTaskRunner.ExecInThread(() =>
        // {
        //     RunTest3();
        // });

        EasyTaskExecQueueMgr.Instance.AddTask(RunTest1);
        EasyTaskExecQueueMgr.Instance.AddTask(RunTest4);

        // RunTest().Trigger();

        // cancellationTokenSource = new CancellationTokenSource();
        // cancellationTokenSource1 = new CancellationTokenSource();
        // RunTest5();
    }

    // [Sirenix.OdinInspector.Button]
    public void TestThread()
    {
        Task.Run(() =>
        {
            RunTest3();
        });
    }

    // [Sirenix.OdinInspector.Button]
    public void Cancel()
    {
        token?.Cancel();

        // if (!cancellationTokenSource.IsCancellationRequested)
        // {
        //     cancellationTokenSource.Cancel();
        // }
        // else if (!cancellationTokenSource1.IsCancellationRequested)
        // {
        //     cancellationTokenSource1.Cancel();
        // }
        UnityEngine.Debug.Log("Cancel");
    }

    public void Update()
    {
        EasyTaskRunner.Tick(Timing.Main);
    }

    public async EasyVoidTask RunTest()
    {
        var task1 = EasyDelayTask.Create().SetDelayTime(1000).SetCancellationToken(token);
        var task2 = EasyDelayTask.Create().SetDelayTime(1000).SetCancellationToken(token);
        var task3 = EasyDelayTask.Create().SetDelayTime(1000).SetCancellationToken(token);
        var task4 = EasyDelayTask.Create().SetDelayTime(1000).SetCancellationToken(token);

        await task1;
        UnityEngine.Debug.Log("task1");
        await task2;
        UnityEngine.Debug.Log("task2");
        await task3;
        UnityEngine.Debug.Log("task3");
        await task4;
        UnityEngine.Debug.Log("task4");

    }

    public async EasyEmptyTask RunTest1()
    {
        await EasyYieldTask.Create().SetCancellationToken(token);
        UnityEngine.Debug.Log("RunYeild");
        await EasyDelayTask.Create().SetDelayTime(1000).SetCancellationToken(token);
        UnityEngine.Debug.Log("RunDelay1");
        await EasyDelayTask.Create().SetDelayTime(2000).SetCancellationToken(token);
        UnityEngine.Debug.Log("RunDelay2");
        var task = EasyTaskRunner.WaitAny(new[] { EasyDelayTask.Create().SetDelayTime(5000).SetCancellationToken(token), EasyDelayTask.Create().SetDelayTime(6000) });
        task.Retain();
        await task;
        await task;
        UnityEngine.Debug.Log("RunDelay3 " + task.GetResult());
        await EasyTaskRunner.WaitAll(new[] { EasyDelayTask.Create().SetDelayTime(5000), EasyDelayTask.Create().SetDelayTime(6000) });
        UnityEngine.Debug.Log("RunDelay4 " + task.GetResult());
        task.Release();
        // RunTest2();
    }

    public async void RunTest2()
    {
        int count = 10;
        while (count > 0)
        {
            if (token?.IsCanceled == true)
            {
                break;
            }
            await EasyTaskRunner.Yield();
            UnityEngine.Debug.Log("RunYeild " + count);
            count--;
            await EasyTaskRunner.Delay(500, token);
        }

    }

    public async void RunTest3()
    {
        UnityEngine.Debug.Log("RunTest3_1 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await RunTest4();
        UnityEngine.Debug.Log("RunTest3_2 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await EasyTaskRunner.Delay(1000, token);
        UnityEngine.Debug.Log("RunTest3_3 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await EasyTaskRunner.Yield();
        UnityEngine.Debug.Log("RunTest3_4 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
    }

    public async EasyEmptyTask RunTest4()
    {
        UnityEngine.Debug.Log("RunTest4_1 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await EasyTaskRunner.Delay(1000, token);
        UnityEngine.Debug.Log("RunTest4_2 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await EasyTaskRunner.Yield();
        UnityEngine.Debug.Log("RunTest4_3 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
    }

    public async void RunTest5()
    {
        UnityEngine.Debug.Log("RunTest5_1 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await RunTest6();
        UnityEngine.Debug.Log("RunTest5_2 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
    }

    public async Task RunTest6()
    {
        UnityEngine.Debug.Log("RunTest6_1 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        var task1 = Task.Delay(1000, cancellationTokenSource.Token);
        var task2 = Task.Delay(10000, cancellationTokenSource1.Token);

        await task1;
        UnityEngine.Debug.Log("RunTest6_2 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await task2;
        
        await Task.Yield();
        UnityEngine.Debug.Log("RunTest6_3 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
    }
}