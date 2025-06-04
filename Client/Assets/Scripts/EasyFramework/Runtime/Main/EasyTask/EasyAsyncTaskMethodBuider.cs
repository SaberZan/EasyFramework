using System;
using System.Runtime.CompilerServices;
using System.Threading;

public struct EasyAsyncGenericTaskMethodBuider
{
    private EasyVoidTask easyTask;

    public EasyVoidTask Task => easyTask;

    public static EasyAsyncGenericTaskMethodBuider Create()
    {
        EasyAsyncGenericTaskMethodBuider builder = new EasyAsyncGenericTaskMethodBuider() { easyTask = (EasyVoidTask)EasyVoidTask.Create(true) };
        return builder;
    }

    public void SetException(Exception exception)
    {
        easyTask.SetException(exception);
    }

    public void SetResult()
    {
        easyTask.SetResult(null);
    }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
    }

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }
}

public class EasyAsyncGenericTaskMethodBuider<T>
{
    private EasyTask<T> easyTask;

    public EasyTask<T> Task => easyTask;

    public static EasyAsyncGenericTaskMethodBuider<T> Create()
    {
        EasyAsyncGenericTaskMethodBuider<T> builder = new EasyAsyncGenericTaskMethodBuider<T>() { easyTask = EasyTask<T>.Create(true) };
        return builder;
    }

    public void SetException(Exception exception)
    {
        easyTask.SetException(exception);
    }

    public void SetResult(T ret)
    {
        easyTask.SetResult(ret);
    }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
    }

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }
}