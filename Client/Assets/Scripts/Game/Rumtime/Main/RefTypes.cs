using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Easy;
using Easy.EasyAsset;
using Easy.AA;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;
using ProtoBuf;

[assembly: Preserve]
enum IntEnum : int
{
    A,
    B,
}

public class RefTypes : MonoBehaviour
{

    void RefUnityEngine()
    {
        GameObject.Instantiate<GameObject>(null);
        Instantiate<GameObject>(null, null);
        Instantiate<GameObject>(null, null, false);
        Instantiate<GameObject>(null, new Vector3(), new Quaternion());
        Instantiate<GameObject>(null, new Vector3(), new Quaternion(), null);
        this.gameObject.AddComponent<RefTypes>();
        gameObject.AddComponent(typeof(RefTypes));
        
        this.gameObject.GetOrAddComponent<RefTypes>();
        this.gameObject.GetOrAddComponent(typeof(RefTypes));

        UnityEngine.Object.FindObjectOfType<UnityEngine.Object>();
        UnityEngine.Resources.Load<UnityEngine.Object>("");
    }
    
    void RefAssetHandle()
    {
        new Easy.AA.SingleUnityAssetHandle<UnityEngine.Object>();
        new Easy.AA.SingleUnityAssetHandle<TextAsset>();
        new Easy.AA.SingleUnityAssetHandle<Sprite>();
        new Easy.AA.SingleUnityAssetHandle<Texture>();
        new Easy.AA.SingleUnityAssetHandle<GameObject>();
        new Easy.AA.SingleUnityAssetHandle<Animation>();
        new Easy.AA.SingleUnityAssetHandle<Animator>();
        new Easy.AA.MultiUnityAssetHandle();
        new Easy.AA.MultiRawAssetHandle();

        new Easy.EasyAsset.SingleUnityAssetHandle<UnityEngine.Object>();
        new Easy.EasyAsset.SingleUnityAssetHandle<TextAsset>();
        new Easy.EasyAsset.SingleUnityAssetHandle<Sprite>();
        new Easy.EasyAsset.SingleUnityAssetHandle<Texture>();
        new Easy.EasyAsset.SingleUnityAssetHandle<GameObject>();
        new Easy.EasyAsset.SingleUnityAssetHandle<Animation>();
        new Easy.EasyAsset.SingleUnityAssetHandle<Animator>();
        new Easy.EasyAsset.MultiUnityAssetHandle();
        new Easy.EasyAsset.MultiRawAssetHandle();
    }
    
    void RefFileMgr()
    {
        FileMgr.Instance.SetTargetClassObjectToFile<System.Object>(null, null, true);
        FileMgr.Instance.GetTargetClassObject<System.Object>(null, true, true);
    }

    void RefNullable()
    {
        // nullable
        object b = null;
        int? a = 5;
        b = a;
        int d = (int?) b ?? 7;
        int e = (int) b;
        a = d;
        b = a;
        b = Enumerable.Range(0, 1).Reverse().Take(1).TakeWhile(x => true).Skip(1).All(x => true);
        b = new WaitForSeconds(1f);
        b = new WaitForSecondsRealtime(1f);
        b = new WaitForFixedUpdate();
        b = new WaitForEndOfFrame();
        b = new WaitWhile(() => true);
        b = new WaitUntil(() => true);
    }


    
    void RefAction()
    {
        Action<int> Action1;
        Action<long> Action2;
        Action<string> Action3;
        Action<float> Action4;
        Action<double> Action5;
        Action<short> Action6;
        Action<byte> Action7;

        Action<byte, byte> Action21;
        Action<byte, int > Action22;
        Action<int, byte> Action23;
        Action<int, int > Action24;
        Action<int, long> Action25;
        Action<int, object > Action26;
        Action<long, int> Action27;
        Action<long, long> Action28;
        Action<long, object> Action29;
        Action<object, long> Action30;
        Action<object, object > Action31;

        EventHandler<System.Object> handle1;
        EventHandler<int> handle2;
        EventHandler<float> handle3;
        EventHandler<string> handle4;
        EventHandler<double> handle5;
        EventHandler<short> handle6;
        EventHandler<byte> handle7;
        EventHandler<long> handle8;
        
        Func<int> func1;
        Func<long> func2;
        Func<string> func3;
        Func<float> func4;
        Func<double> func5;
        Func<short> func6;
        Func<byte> func7;
    }

    public delegate void RefEventHandle(RefEventArg eventArg);
    public abstract class RefEventArg
    {
        public bool isUsed = false;
        public abstract void Reset();
    }
    
    void RefExpressions()
    {
        Expression.Lambda<System.Object>(null,new ParameterExpression[]{});
        Expression.Lambda<RefEventHandle>(null,new ParameterExpression[]{});
    }
    
    void RefContainer()
    {
        var newList =  new List<object>()
        {
            new Dictionary<byte, byte>(),
            new Dictionary<byte, int>(),
            new Dictionary<int, byte>(),
            new Dictionary<int, int>(),
            new Dictionary<int, long>(),
            new Dictionary<int, object>(),
            new Dictionary<long, int>(),
            new Dictionary<long, long>(),
            new Dictionary<long, object>(),
            new Dictionary<string,object>(),
            new Dictionary<object, long>(),
            new Dictionary<object, object>(),
            new Dictionary<object, string>(),
            new Dictionary<object, int>(),
            new Dictionary<object, float>(),
            new SortedDictionary<int, long>(),
            new SortedDictionary<int, object>(),
            new SortedDictionary<long, int>(),
            new SortedDictionary<long, object>(),
            new HashSet<byte>(),
            new HashSet<int>(),
            new HashSet<long>(),
            new HashSet<object>(),
            new List<int>(),
            new List<long>(),
            new List<float>(),
            new List<double>(),
            new List<object>(),
            new ValueTuple<int, int>(1, 1),
            new ValueTuple<long, long>(1, 1),
            new ValueTuple<object, object>(1, 1),
        };

        newList.Pop();
        newList.Push<object>(null);

    }

    void RefNewtonsoftJson()
    {
        // AotHelper.EnsureList<int>();
        // AotHelper.EnsureList<long>();
        // AotHelper.EnsureList<float>();
        // AotHelper.EnsureList<double>();
        // AotHelper.EnsureList<string>();
        // AotHelper.EnsureDictionary<int, int>();
        // AotHelper.EnsureDictionary<int, string>();

        JsonConvert.DeserializeObject<System.Object>("");
        JsonConvert.DeserializeObject<System.Object>("", new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
    }
    
    class RefStateMachine : IAsyncStateMachine
    {
        public void MoveNext()
        {
            throw new NotImplementedException();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            throw new NotImplementedException();
        }
    }

    void RefAsyncMethod()
    {
        var stateMachine = new RefStateMachine();

        TaskAwaiter aw = default;
        var c0 = new AsyncTaskMethodBuilder();
        c0.Start(ref stateMachine);
        c0.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c0.SetException(null);
        c0.SetResult();

        var c1 = new AsyncTaskMethodBuilder();
        c1.Start(ref stateMachine);
        c1.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c1.SetException(null);
        c1.SetResult();

        var c2 = new AsyncTaskMethodBuilder<bool>();
        c2.Start(ref stateMachine);
        c2.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c2.SetException(null);
        c2.SetResult(default);

        var c3 = new AsyncTaskMethodBuilder<int>();
        c3.Start(ref stateMachine);
        c3.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c3.SetException(null);
        c3.SetResult(default);

        var c4 = new AsyncTaskMethodBuilder<long>();
        c4.Start(ref stateMachine);
        c4.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c4.SetException(null);

        var c5 = new AsyncTaskMethodBuilder<float>();
        c5.Start(ref stateMachine);
        c5.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c5.SetException(null);
        c5.SetResult(default);

        var c6 = new AsyncTaskMethodBuilder<double>();
        c6.Start(ref stateMachine);
        c6.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c6.SetException(null);
        c6.SetResult(default);

        var c7 = new AsyncTaskMethodBuilder<object>();
        c7.Start(ref stateMachine);
        c7.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c7.SetException(null);
        c7.SetResult(default);

        var c8 = new AsyncTaskMethodBuilder<IntEnum>();
        c8.Start(ref stateMachine);
        c8.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c8.SetException(null);
        c8.SetResult(default);

        var c9 = new AsyncVoidMethodBuilder();
        var b = AsyncVoidMethodBuilder.Create();
        c9.Start(ref stateMachine);
        c9.AwaitUnsafeOnCompleted(ref aw, ref stateMachine);
        c9.SetException(null);
        c9.SetResult();
        Debug.Log(b);
    }
    
    public class MyComparer<T> : Comparer<T>
    {
        public override int Compare(T x, T y)
        {
            return 0;
        }
    }
    
    void RefComparers()
    {
        var a = new object[]
        {
            new MyComparer<int>(),
            new MyComparer<long>(),
            new MyComparer<float>(),
            new MyComparer<double>(),
            new MyComparer<object>(),
        };

        new MyComparer<int>().Compare(default, default);
        new MyComparer<long>().Compare(default, default);
        new MyComparer<float>().Compare(default, default);
        new MyComparer<double>().Compare(default, default);
        new MyComparer<object>().Compare(default, default);

        object b = EqualityComparer<int>.Default;
        b = EqualityComparer<long>.Default;
        b = EqualityComparer<float>.Default;
        b = EqualityComparer<double>.Default;
        b = EqualityComparer<object>.Default;
    }

    [ProtoContract]
    public class ProtoDemo
    {
        [ProtoMember(1)]
        public int a = 0;

        [ProtoMap]
        [ProtoMember(2)]
        public Dictionary<string,int> b = new Dictionary<string,int>();

        [ProtoMap]
        [ProtoMember(3)]
        public Dictionary<string,object> c = new Dictionary<string, object>();

        void ProtoBufFun()
        {
            MemoryStream elementStream = new MemoryStream(new byte[0]);
            int tmp = ProtoBuf.Serializer.Deserialize<int>(elementStream);
            ProtoBuf.Serializer.Serialize<int>(elementStream, tmp);
        }
    }

    [ProtoInclude(1,typeof(ProtoDemo))]
    [ProtoContract]
    public class ProtoDemo1 : ProtoDemo
    {

    }

    [ProtoContract]
    public enum ProtoType
    {
        [ProtoEnum(Name = "aaa")]
        aaa,
    }

}
