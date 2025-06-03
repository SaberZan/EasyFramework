using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EasyTaskTest : MonoBehaviour
{
    private EasyCancellationToken token;

    public void Awake()
    {
        Thread thread = new Thread(ThreadStart);
        thread.Start();
    }

    public void Test()
    {
        token = new EasyCancellationToken();
        RunTest();
    }

    public void Cancel()
    {
        token?.Cancel();
    }

    public void Update()
    {
        EasyTaskRunner.Tick(Timing.Main);
    }

    public void ThreadStart()
    {
        EasyTaskRunner.Tick(Timing.Thread);
        Thread.Sleep(15);
    }

    public async void RunTest()
    {
        await RunTest1();
        UnityEngine.Debug.Log("RunTest");
    }

    public async EasyVoidTask RunTest1()
    {
        await EasyYeildTask.Create().SetCancellationToken(token);
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
            await EasyTaskRunner.Yeild();
            UnityEngine.Debug.Log("RunYeild " + count);
            count--;
            await EasyTaskRunner.Delay(0.5f, token);
        }

        RunTest3();
    }

    public async void RunTest3()
    {
        await EasyTaskRunner.Delay(1f, token).SetTiming(Timing.Thread);
        UnityEngine.Debug.Log("RunThreadDelay");

        await EasyTaskRunner.Yeild();
        UnityEngine.Debug.Log("RunYeild ");
    }
}