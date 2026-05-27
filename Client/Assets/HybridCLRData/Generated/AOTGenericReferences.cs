using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"EasyFramework.Main.dll",
		"Newtonsoft.Json.dll",
		"SQLite4Unity.dll",
		"System.Core.dll",
		"Toolbox.dll",
		"UnityEngine.CoreModule.dll",
		"UnityEngine.JSONSerializeModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Easy.EasyAsyncGenericTaskMethodBuider<object>
	// Easy.EasyTask<object>
	// Easy.ISingleUnityAssetHandle<object>
	// Easy.OrderIndexAttribute.<>c__DisplayClass2_0<object>
	// Easy.SerializableDictionary<object,object>
	// Easy.SerializableDictionaryBase<object,object,object>
	// Google.FlatBuffers.Offset<CfgSpace.BoolArray>
	// Google.FlatBuffers.Offset<CfgSpace.FloatArray>
	// Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>
	// Google.FlatBuffers.Offset<CfgSpace.GameConstCfgs>
	// Google.FlatBuffers.Offset<CfgSpace.IntArray>
	// Google.FlatBuffers.Offset<CfgSpace.StringArray>
	// SQLite.AsyncTableQuery.<>c__DisplayClass14_0<object>
	// SQLite.AsyncTableQuery.<>c__DisplayClass15_0<object>
	// SQLite.AsyncTableQuery.<>c__DisplayClass18_0<object>
	// SQLite.AsyncTableQuery.<>c__DisplayClass19_0<object>
	// SQLite.AsyncTableQuery.<>c__DisplayClass20_0<object>
	// SQLite.AsyncTableQuery<object>
	// SQLite.SQLiteAsyncConnection.<>c__DisplayClass32_0<object>
	// SQLite.SQLiteAsyncConnection.<>c__DisplayClass74_0<object>
	// SQLite.TableQuery.<>c<object>
	// SQLite.TableQuery.CompileResult<object>
	// SQLite.TableQuery<object>
	// System.Action<TextPro.SpriteInfo>
	// System.Action<UnityEngine.Rect>
	// System.Action<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Action<float>
	// System.Action<int>
	// System.Action<object,object>
	// System.Action<object>
	// System.ArraySegment.Enumerator<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>
	// System.ArraySegment.Enumerator<Google.FlatBuffers.StringOffset>
	// System.ArraySegment.Enumerator<byte>
	// System.ArraySegment.Enumerator<float>
	// System.ArraySegment.Enumerator<int>
	// System.ArraySegment.Enumerator<ushort>
	// System.ArraySegment<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>
	// System.ArraySegment<Google.FlatBuffers.StringOffset>
	// System.ArraySegment<byte>
	// System.ArraySegment<float>
	// System.ArraySegment<int>
	// System.ArraySegment<ushort>
	// System.ByReference<ushort>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<object>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<object>
	// System.Collections.Concurrent.ConcurrentQueue<object>
	// System.Collections.Concurrent.ConcurrentStack.<GetEnumerator>d__35<object>
	// System.Collections.Concurrent.ConcurrentStack.Node<object>
	// System.Collections.Concurrent.ConcurrentStack<object>
	// System.Collections.Generic.ArraySortHelper<TextPro.SpriteInfo>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Rect>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.ArraySortHelper<float>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<TextPro.SpriteInfo>
	// System.Collections.Generic.Comparer<UnityEngine.Rect>
	// System.Collections.Generic.Comparer<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.Comparer<float>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.ComparisonComparer<TextPro.SpriteInfo>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.Rect>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.ComparisonComparer<float>
	// System.Collections.Generic.ComparisonComparer<int>
	// System.Collections.Generic.ComparisonComparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<CfgSpace.GameConstCfg>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,CfgSpace.GameConstCfg>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<TextPro.SpriteInfo>
	// System.Collections.Generic.ICollection<UnityEngine.Rect>
	// System.Collections.Generic.ICollection<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<UnityEngine.UIVertex>
	// System.Collections.Generic.ICollection<float>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<TextPro.SpriteInfo>
	// System.Collections.Generic.IComparer<UnityEngine.Rect>
	// System.Collections.Generic.IComparer<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.IComparer<float>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,CfgSpace.GameConstCfg>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<TextPro.SpriteInfo>
	// System.Collections.Generic.IEnumerable<UnityEngine.Rect>
	// System.Collections.Generic.IEnumerable<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<float>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,CfgSpace.GameConstCfg>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<TextPro.SpriteInfo>
	// System.Collections.Generic.IEnumerator<UnityEngine.Rect>
	// System.Collections.Generic.IEnumerator<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<float>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<TextPro.SpriteInfo>
	// System.Collections.Generic.IList<UnityEngine.Rect>
	// System.Collections.Generic.IList<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.IList<UnityEngine.UIVertex>
	// System.Collections.Generic.IList<float>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<TextPro.SpriteInfo>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Rect>
	// System.Collections.Generic.List.Enumerator<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.List.Enumerator<float>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<TextPro.SpriteInfo>
	// System.Collections.Generic.List<UnityEngine.Rect>
	// System.Collections.Generic.List<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.List<float>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<TextPro.SpriteInfo>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Rect>
	// System.Collections.Generic.ObjectComparer<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectComparer<float>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<CfgSpace.GameConstCfg>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<TextPro.SpriteInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Rect>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<float>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<TextPro.SpriteInfo>
	// System.Comparison<UnityEngine.Rect>
	// System.Comparison<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Comparison<float>
	// System.Comparison<int>
	// System.Comparison<object>
	// System.Func<int>
	// System.Func<object,byte>
	// System.Func<object,int>
	// System.Func<object,object,int>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Linq.Buffer<object>
	// System.Linq.Enumerable.<SelectManyIterator>d__23<object,object,object>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,object>
	// System.Linq.Enumerable.WhereSelectListIterator<object,object>
	// System.Nullable<CfgSpace.GameConstCfg>
	// System.Nullable<System.DateTimeOffset>
	// System.Nullable<int>
	// System.Predicate<TextPro.SpriteInfo>
	// System.Predicate<UnityEngine.Rect>
	// System.Predicate<UnityEngine.SerializedDictionary.KeyValuePair<object,object>>
	// System.Predicate<float>
	// System.Predicate<int>
	// System.Predicate<object>
	// System.ReadOnlySpan.Enumerator<ushort>
	// System.ReadOnlySpan<ushort>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<int>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<int>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<int>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Span.Enumerator<ushort>
	// System.Span<ushort>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<int>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task<int>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory.<>c<int>
	// System.Threading.Tasks.TaskFactory.<>c<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass32_0<int>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass32_0<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<int>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory<int>
	// System.Threading.Tasks.TaskFactory<object>
	// UnityEngine.Events.InvokableCall<UnityEngine.Vector2>
	// UnityEngine.Events.InvokableCall<object>
	// UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene,int>
	// UnityEngine.Events.UnityAction<UnityEngine.Vector2>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityEvent<UnityEngine.Vector2>
	// UnityEngine.Events.UnityEvent<object>
	// UnityEngine.SerializedDictionary.KeyValuePair<object,object>
	// UnityEngine.SerializedDictionary<object,object>
	// }}

	public void RefMethods()
	{
		// Easy.ISingleUnityAssetHandle<object> Easy.AssetsLoader.LoadUnityAssetByPath<object>(string)
		// Easy.IMultiUnityAssetHandle Easy.AssetsLoader.LoadUnityAssetsByPaths<object>(System.Collections.Generic.IEnumerable<string>)
		// Easy.ISingleUnityAssetHandle<object> Easy.AssetsMgr.LoadAsset<object>(string)
		// Easy.IMultiUnityAssetHandle Easy.AssetsMgr.LoadAssetsByPath<object>(System.Collections.Generic.IEnumerable<string>)
		// System.Void Easy.EasyAsyncEmptyTaskMethodBuider.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<int>,object>(System.Runtime.CompilerServices.TaskAwaiter<int>&,object&)
		// System.Void Easy.EasyAsyncEmptyTaskMethodBuider.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void Easy.EasyAsyncEmptyTaskMethodBuider.Start<object>(object&)
		// System.Void Easy.EasyAsyncGenericTaskMethodBuider<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void Easy.EasyAsyncGenericTaskMethodBuider<object>.Start<object>(object&)
		// System.Void Easy.EasyAsyncVoidMethodBuider.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<int>,object>(System.Runtime.CompilerServices.TaskAwaiter<int>&,object&)
		// System.Void Easy.EasyAsyncVoidMethodBuider.Start<object>(object&)
		// object Easy.EasyFrameworkConfig.GetEasyConfig<object>()
		// object Easy.GameObjectExtension.GetOrAddComponent<object>(UnityEngine.GameObject)
		// bool Easy.GameObjectExtension.HasComponent<object>(UnityEngine.GameObject)
		// System.Collections.Generic.Dictionary<int,System.Collections.Generic.List<object>> Easy.OrderIndexAttribute.GetBatchListByInterval<object>(System.Collections.Generic.List<object>,int)
		// System.Void Easy.OrderIndexAttribute.Sort<object>(System.Collections.Generic.List<object>)
		// object Easy.ProxyMgr.Get<object>()
		// int Google.FlatBuffers.ByteBuffer.ArraySize<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>(System.ArraySegment<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>)
		// int Google.FlatBuffers.ByteBuffer.ArraySize<Google.FlatBuffers.StringOffset>(System.ArraySegment<Google.FlatBuffers.StringOffset>)
		// int Google.FlatBuffers.ByteBuffer.ArraySize<byte>(System.ArraySegment<byte>)
		// int Google.FlatBuffers.ByteBuffer.ArraySize<byte>(byte[])
		// int Google.FlatBuffers.ByteBuffer.ArraySize<float>(System.ArraySegment<float>)
		// int Google.FlatBuffers.ByteBuffer.ArraySize<float>(float[])
		// int Google.FlatBuffers.ByteBuffer.ArraySize<int>(System.ArraySegment<int>)
		// int Google.FlatBuffers.ByteBuffer.ArraySize<int>(int[])
		// bool Google.FlatBuffers.ByteBuffer.IsSupportedType<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>()
		// bool Google.FlatBuffers.ByteBuffer.IsSupportedType<Google.FlatBuffers.StringOffset>()
		// bool Google.FlatBuffers.ByteBuffer.IsSupportedType<byte>()
		// bool Google.FlatBuffers.ByteBuffer.IsSupportedType<float>()
		// bool Google.FlatBuffers.ByteBuffer.IsSupportedType<int>()
		// int Google.FlatBuffers.ByteBuffer.Put<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>(int,System.ArraySegment<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>)
		// int Google.FlatBuffers.ByteBuffer.Put<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>(int,System.IntPtr,int)
		// int Google.FlatBuffers.ByteBuffer.Put<Google.FlatBuffers.StringOffset>(int,System.ArraySegment<Google.FlatBuffers.StringOffset>)
		// int Google.FlatBuffers.ByteBuffer.Put<Google.FlatBuffers.StringOffset>(int,System.IntPtr,int)
		// int Google.FlatBuffers.ByteBuffer.Put<byte>(int,System.ArraySegment<byte>)
		// int Google.FlatBuffers.ByteBuffer.Put<byte>(int,System.IntPtr,int)
		// int Google.FlatBuffers.ByteBuffer.Put<float>(int,System.ArraySegment<float>)
		// int Google.FlatBuffers.ByteBuffer.Put<float>(int,System.IntPtr,int)
		// int Google.FlatBuffers.ByteBuffer.Put<int>(int,System.ArraySegment<int>)
		// int Google.FlatBuffers.ByteBuffer.Put<int>(int,System.IntPtr,int)
		// int Google.FlatBuffers.ByteBuffer.SizeOf<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>()
		// int Google.FlatBuffers.ByteBuffer.SizeOf<Google.FlatBuffers.StringOffset>()
		// int Google.FlatBuffers.ByteBuffer.SizeOf<byte>()
		// int Google.FlatBuffers.ByteBuffer.SizeOf<float>()
		// int Google.FlatBuffers.ByteBuffer.SizeOf<int>()
		// byte[] Google.FlatBuffers.ByteBuffer.ToArray<byte>(int,int)
		// float[] Google.FlatBuffers.ByteBuffer.ToArray<float>(int,int)
		// int[] Google.FlatBuffers.ByteBuffer.ToArray<int>(int,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>(Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>[])
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>(System.ArraySegment<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>(System.IntPtr,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<Google.FlatBuffers.StringOffset>(Google.FlatBuffers.StringOffset[])
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<Google.FlatBuffers.StringOffset>(System.ArraySegment<Google.FlatBuffers.StringOffset>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<Google.FlatBuffers.StringOffset>(System.IntPtr,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<byte>(System.ArraySegment<byte>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<byte>(System.IntPtr,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<byte>(byte[])
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<float>(System.ArraySegment<float>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<float>(System.IntPtr,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<float>(float[])
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<int>(System.ArraySegment<int>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<int>(System.IntPtr,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Add<int>(int[])
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>(System.ArraySegment<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>(System.IntPtr,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<Google.FlatBuffers.StringOffset>(System.ArraySegment<Google.FlatBuffers.StringOffset>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<Google.FlatBuffers.StringOffset>(System.IntPtr,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<byte>(System.ArraySegment<byte>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<byte>(System.IntPtr,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<float>(System.ArraySegment<float>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<float>(System.IntPtr,int)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<int>(System.ArraySegment<int>)
		// System.Void Google.FlatBuffers.FlatBufferBuilder.Put<int>(System.IntPtr,int)
		// byte[] Google.FlatBuffers.Table.__vector_as_array<byte>(int)
		// float[] Google.FlatBuffers.Table.__vector_as_array<float>(int)
		// int[] Google.FlatBuffers.Table.__vector_as_array<int>(int)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string,Newtonsoft.Json.JsonSerializerSettings)
		// System.Threading.Tasks.Task<SQLite.TableMapping> SQLite.SQLiteAsyncConnection.GetMappingAsync<object>(SQLite.CreateFlags)
		// System.Threading.Tasks.Task<object> SQLite.SQLiteAsyncConnection.ReadAsync<object>(System.Func<SQLite.SQLiteConnectionWithLock,object>)
		// SQLite.AsyncTableQuery<object> SQLite.SQLiteAsyncConnection.Table<object>()
		// SQLite.TableQuery<object> SQLite.SQLiteConnection.Table<object>()
		// object System.Activator.CreateInstance<object>()
		// object[] System.Array.Empty<object>()
		// System.Void System.Array.Resize<byte>(byte[]&,int)
		// int System.Linq.Enumerable.Count<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectMany<object,object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<object>>,System.Func<object,object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectManyIterator<object,object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<object>>,System.Func<object,object,object>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.List<int> System.Linq.Enumerable.ToList<int>(System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<object>.Select<object>(System.Func<object,object>)
		// object System.Reflection.CustomAttributeExtensions.GetCustomAttribute<object>(System.Reflection.MemberInfo)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<object,object>(object&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<object>(object&)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// System.Threading.Tasks.Task<object> System.Threading.Tasks.TaskFactory.StartNew<object>(System.Func<object>,System.Threading.CancellationToken,System.Threading.Tasks.TaskCreationOptions,System.Threading.Tasks.TaskScheduler)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object UnityEngine.JsonUtility.FromJson<object>(string)
		// object UnityEngine.Object.FindAnyObjectByType<object>()
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
	}
}