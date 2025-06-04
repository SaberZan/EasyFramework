using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        RunTest();
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
        await RunTest4();

        await EasyTaskRunner.Delay(1f, token);
        UnityEngine.Debug.Log("RunThreadDelay");

        await EasyTaskRunner.Yield();
        UnityEngine.Debug.Log("RunYeild ");
    }

    public async EasyVoidTask RunTest4()
    {
        await EasyTaskRunner.Delay(1f, token);
        UnityEngine.Debug.Log("RunThreadDelay4");
        transform.position = new Vector3(0, 0, 0);
        int a = 1;
        await EasyTaskRunner.Yield();
        UnityEngine.Debug.Log("RunYeild4 ");
    }

    public async void RunTest5()
    {
        await RunTest6();
    }

    public async Task RunTest6()
    {
        await Task.Delay(1000);
        UnityEngine.Debug.Log("RunThreadDelay6");
        transform.position = new Vector3(0, 0, 0);
        int a = 1;
        await Task.Yield();
        UnityEngine.Debug.Log("RunYeild6 ");
    }
}