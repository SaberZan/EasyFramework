using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EasyTaskTest : MonoBehaviour
{
    private EasyCancellationToken token;

    public void Awake()
    {
        EasyTaskRunner.Log += Debug.LogError;

        EasyTaskRunner.StartThreadTiming();

    }

    // [Sirenix.OdinInspector.Button]
    public void Test()
    {
        token = new EasyCancellationToken();
        EasyTaskRunner.ExecInThread(() =>
        {
            RunTest3();
        });
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
    }

    public void Update()
    {
        EasyTaskRunner.Tick(Timing.Main);
    }

    public async void RunTest()
    {
        await RunTest1();
        UnityEngine.Debug.Log("RunTest");
    }

    public async EasyVoidTask RunTest1()
    {
        await EasyYieldTask.Create().SetCancellationToken(token);
        UnityEngine.Debug.Log("RunYeild");
        await EasyDelayTask.Create().SetDelayTime(1).SetCancellationToken(token);
        UnityEngine.Debug.Log("RunDelay1");
        await EasyDelayTask.Create().SetDelayTime(2).SetCancellationToken(token);
        UnityEngine.Debug.Log("RunDelay2");
        await EasyTaskRunner.WaitAll(new[] { EasyDelayTask.Create().SetDelayTime(5), EasyDelayTask.Create().SetDelayTime(1) });
        UnityEngine.Debug.Log("RunDelay3");
        await EasyTaskRunner.WhenAny(new[] { EasyDelayTask.Create().SetDelayTime(5), EasyDelayTask.Create().SetDelayTime(1) });
        UnityEngine.Debug.Log("RunDelay4");
        RunTest2();
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
            await EasyTaskRunner.Delay(0.5f, token);
        }

    }

    public async void RunTest3()
    {
        UnityEngine.Debug.Log("RunTest3_1 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await RunTest4();
        UnityEngine.Debug.Log("RunTest3_2 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await EasyTaskRunner.Delay(1f, token);
        UnityEngine.Debug.Log("RunTest3_3 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await EasyTaskRunner.Yield();
        UnityEngine.Debug.Log("RunTest3_4 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
    }

    public async EasyVoidTask RunTest4()
    {
        UnityEngine.Debug.Log("RunTest4_1 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await EasyTaskRunner.Delay(1f, token);
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
        await Task.Delay(1000);
        UnityEngine.Debug.Log("RunTest6_2 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
        await Task.Yield();
        UnityEngine.Debug.Log("RunTest6_3 ThreadId == " + Thread.CurrentThread.ManagedThreadId);
    }
}