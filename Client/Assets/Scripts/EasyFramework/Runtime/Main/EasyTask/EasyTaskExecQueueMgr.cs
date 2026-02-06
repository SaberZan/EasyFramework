using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Easy;

[PreInit]
[OrderIndex((int)PreInitOrderIndexEnum.EasyTaskExecQueueMgr)]
public class EasyTaskExecQueueMgr : Singleton<EasyTaskExecQueueMgr>
{

    private Queue<Func<EasyTask<object>>> _taskQueue = new Queue<Func<EasyTask<object>>>();

    private int pause = 0;

    private EasyCancellationToken easyCancellationToken;

    public override void BeforeRestart()
    {
        Exit();
    }

    public override void Init(InitCompleteCallback complete)
    {
        Start();
        complete?.Invoke(true);
    }

    public async void Start()
    {
        easyCancellationToken = new EasyCancellationToken();
        await Exec(easyCancellationToken).SetTag("ExecQueue");
    }

    public void Exit()
    {
        easyCancellationToken?.Cancel();
        easyCancellationToken = null;
        _taskQueue.Clear();
        pause = 0;
    }

    public async EasyVoidTask Exec(EasyCancellationToken token)
    {
        while (true)
        {
            if (token.IsCanceled)
            {
                break;
            }

            if (pause > 0)
            {
                await EasyTaskRunner.Yield();
                continue;
            }

            if (token.IsCanceled)
            {
                break;
            }
            
            if (_taskQueue.Count > 0)
            {
                var task = _taskQueue.Dequeue();
                await task().SetCancellationToken(token);
            }
            else
            {
                await EasyTaskRunner.Yield();
            }

            if (token.IsCanceled)
            {
                break;
            }
        }
    }

    public void AddTask(Func<EasyTask<object>> task)
    {
        _taskQueue.Enqueue(task);
    }

    public void Pause()
    {
        pause++;
    }

    public void Resume()
    {
        pause--;
    }

}