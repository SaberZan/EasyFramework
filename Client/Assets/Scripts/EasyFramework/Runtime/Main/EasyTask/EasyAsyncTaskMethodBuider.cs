using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Easy
{

    public struct EasyAsyncVoidMethodBuider
    {
        public EasyVoidTask Task => default;

        public static EasyAsyncVoidMethodBuider Create()
        {
            EasyAsyncVoidMethodBuider builder = new EasyAsyncVoidMethodBuider() { };
            return builder;
        }

        public void SetException(Exception exception)
        {
 
        }

        public void SetResult()
        {
   
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

    public struct EasyAsyncEmptyTaskMethodBuider
    {
        private EasyEmptyTask easyTask;

        public EasyEmptyTask Task => easyTask;

        public static EasyAsyncEmptyTaskMethodBuider Create()
        {
            EasyAsyncEmptyTaskMethodBuider builder = new EasyAsyncEmptyTaskMethodBuider() { easyTask = (EasyEmptyTask)EasyEmptyTask.Create(true) };
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
            if (Task.EasyCancellationToken?.IsCanceled == true)
            {
                if (awaiter is IEasyTaskInterface task)
                {
                    task.SetException(new EasyCancelException());
                }
            }
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            if (Task.EasyCancellationToken?.IsCanceled == true)
            {
                if (awaiter is IEasyTaskInterface task)
                {
                    task.SetException(new EasyCancelException());
                }
            }
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            if (Task.EasyCancellationToken?.IsCanceled == true)
            {

            }
            else
            {
                stateMachine.MoveNext();
            }
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
            if (Task.EasyCancellationToken?.IsCanceled == true)
            {
                if (awaiter is IEasyTaskInterface task)
                {
                    task.SetException(new EasyCancelException());
                }
            }
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            if (Task.EasyCancellationToken?.IsCanceled == true)
            {
                if (awaiter is IEasyTaskInterface task)
                {
                    task.SetException(new EasyCancelException());
                }
            }
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            if (Task.EasyCancellationToken?.IsCanceled == true)
            {

            }
            else
            {
                stateMachine.MoveNext();
            }
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}