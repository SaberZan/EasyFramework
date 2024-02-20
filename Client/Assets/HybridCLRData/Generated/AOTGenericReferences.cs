using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"EasyFrameworkMain.dll",
		"Newtonsoft.Json.dll",
		"System.Core.dll",
		"UnityEngine.AndroidJNIModule.dll",
		"UnityEngine.CoreModule.dll",
		"UnityEngine.JSONSerializeModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
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
	// System.Action<int,object>
	// System.Action<int>
	// System.Action<object,object,object>
	// System.Action<object,object>
	// System.Action<object>
	// System.ArraySegment.Enumerator<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>
	// System.ArraySegment.Enumerator<Google.FlatBuffers.StringOffset>
	// System.ArraySegment.Enumerator<UnityEngine.jvalue>
	// System.ArraySegment.Enumerator<byte>
	// System.ArraySegment.Enumerator<float>
	// System.ArraySegment.Enumerator<int>
	// System.ArraySegment.Enumerator<ushort>
	// System.ArraySegment<Google.FlatBuffers.Offset<CfgSpace.GameConstCfg>>
	// System.ArraySegment<Google.FlatBuffers.StringOffset>
	// System.ArraySegment<UnityEngine.jvalue>
	// System.ArraySegment<byte>
	// System.ArraySegment<float>
	// System.ArraySegment<int>
	// System.ArraySegment<ushort>
	// System.ByReference<UnityEngine.jvalue>
	// System.ByReference<ushort>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.ComparisonComparer<int>
	// System.Collections.Generic.ComparisonComparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,float>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,float>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<int,float>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<CfgSpace.GameConstCfg>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,CfgSpace.GameConstCfg>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,CfgSpace.GameConstCfg>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,CfgSpace.GameConstCfg>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<int,float>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<object,CfgSpace.GameConstCfg>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<CfgSpace.GameConstCfg>
	// System.Collections.Generic.ObjectEqualityComparer<float>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<int>
	// System.Comparison<object>
	// System.Func<object,byte>
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
	// System.Predicate<int>
	// System.Predicate<object>
	// System.ReadOnlySpan.Enumerator<UnityEngine.jvalue>
	// System.ReadOnlySpan.Enumerator<ushort>
	// System.ReadOnlySpan<UnityEngine.jvalue>
	// System.ReadOnlySpan<ushort>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Span.Enumerator<UnityEngine.jvalue>
	// System.Span.Enumerator<ushort>
	// System.Span<UnityEngine.jvalue>
	// System.Span<ushort>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory.<>c<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass32_0<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory<object>
	// UnityEngine.Events.InvokableCall<UnityEngine.Vector2>
	// UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene,int>
	// UnityEngine.Events.UnityAction<UnityEngine.Vector2>
	// UnityEngine.Events.UnityAction<UnityEngine.Vector3,UnityEngine.Vector3>
	// UnityEngine.Events.UnityAction<UnityEngine.Vector3>
	// UnityEngine.Events.UnityAction<float>
	// UnityEngine.Events.UnityEvent<UnityEngine.Vector2>
	// }}

	public void RefMethods()
	{
		// Easy.ISingleUnityAssetHandle<object> Easy.AssetsLoader.LoadUnityAsset<object>(string)
		// Easy.ISingleUnityAssetHandle<object> Easy.AssetsMgr.LoadAsset<object>(string)
		// object Easy.EasyFrameworkConfig.GetEasyConfig<object>()
		// object Easy.GameObjectExtension.GetOrAddComponent<object>(UnityEngine.GameObject)
		// bool Easy.GameObjectExtension.HasComponent<object>(UnityEngine.GameObject)
		// object Easy.ListExtension.Pop<object>(System.Collections.Generic.List<object>)
		// System.Void Easy.ListExtension.Push<object>(System.Collections.Generic.List<object>,object)
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
		// object System.Activator.CreateInstance<object>()
		// object[] System.Array.Empty<object>()
		// System.Void System.Array.Resize<byte>(byte[]&,int)
		// int System.Linq.Enumerable.Count<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Empty<object>()
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectMany<object,object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<object>>,System.Func<object,object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectManyIterator<object,object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<object>>,System.Func<object,object,object>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.List<int> System.Linq.Enumerable.ToList<int>(System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<object>.Select<object>(System.Func<object,object>)
		// object System.Reflection.CustomAttributeExtensions.GetCustomAttribute<object>(System.Reflection.MemberInfo)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,Easy.ToastUILayer.<ShowToast>d__3>(System.Runtime.CompilerServices.TaskAwaiter&,Easy.ToastUILayer.<ShowToast>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Easy.FlatBufferConfigLoader.<LoadConfigCacheAsync>d__10<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Easy.FlatBufferConfigLoader.<LoadConfigCacheAsync>d__10<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Easy.FlatBufferConfigLoader.<LoadConfigCacheAsync>d__8>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Easy.FlatBufferConfigLoader.<LoadConfigCacheAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Easy.FlatBufferConfigLoader.<LoadConfigUnCacheAsync>d__12>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Easy.FlatBufferConfigLoader.<LoadConfigUnCacheAsync>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Easy.FlatBufferConfigLoader.<LoadConfigUnCacheAsync>d__14<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Easy.FlatBufferConfigLoader.<LoadConfigUnCacheAsync>d__14<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Easy.JsonConfigLoader.<LoadConfigCacheAsync>d__10<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Easy.JsonConfigLoader.<LoadConfigCacheAsync>d__10<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Easy.JsonConfigLoader.<LoadConfigCacheAsync>d__8>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Easy.JsonConfigLoader.<LoadConfigCacheAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Easy.JsonConfigLoader.<LoadConfigUnCacheAsync>d__12>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Easy.JsonConfigLoader.<LoadConfigUnCacheAsync>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Easy.JsonConfigLoader.<LoadConfigUnCacheAsync>d__14<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Easy.JsonConfigLoader.<LoadConfigUnCacheAsync>d__14<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Easy.FlatBufferConfigLoader.<LoadConfigCacheAsync>d__10<object>>(Easy.FlatBufferConfigLoader.<LoadConfigCacheAsync>d__10<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Easy.FlatBufferConfigLoader.<LoadConfigCacheAsync>d__8>(Easy.FlatBufferConfigLoader.<LoadConfigCacheAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Easy.FlatBufferConfigLoader.<LoadConfigUnCacheAsync>d__12>(Easy.FlatBufferConfigLoader.<LoadConfigUnCacheAsync>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Easy.FlatBufferConfigLoader.<LoadConfigUnCacheAsync>d__14<object>>(Easy.FlatBufferConfigLoader.<LoadConfigUnCacheAsync>d__14<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Easy.JsonConfigLoader.<LoadConfigCacheAsync>d__10<object>>(Easy.JsonConfigLoader.<LoadConfigCacheAsync>d__10<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Easy.JsonConfigLoader.<LoadConfigCacheAsync>d__8>(Easy.JsonConfigLoader.<LoadConfigCacheAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Easy.JsonConfigLoader.<LoadConfigUnCacheAsync>d__12>(Easy.JsonConfigLoader.<LoadConfigUnCacheAsync>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Easy.JsonConfigLoader.<LoadConfigUnCacheAsync>d__14<object>>(Easy.JsonConfigLoader.<LoadConfigUnCacheAsync>d__14<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Easy.ToastUILayer.<ShowToast>d__3>(Easy.ToastUILayer.<ShowToast>d__3&)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// byte UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<byte>(System.IntPtr)
		// int UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<int>(System.IntPtr)
		// object UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<object>(System.IntPtr)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetFieldID<object>(System.IntPtr,string,bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<byte>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<int>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<object>(System.IntPtr,string,object[],bool)
		// byte UnityEngine.AndroidJavaObject.Call<byte>(string,object[])
		// int UnityEngine.AndroidJavaObject.Call<int>(string,object[])
		// object UnityEngine.AndroidJavaObject.Call<object>(string,object[])
		// byte UnityEngine.AndroidJavaObject.FromJavaArrayDeleteLocalRef<byte>(System.IntPtr)
		// int UnityEngine.AndroidJavaObject.FromJavaArrayDeleteLocalRef<int>(System.IntPtr)
		// object UnityEngine.AndroidJavaObject.FromJavaArrayDeleteLocalRef<object>(System.IntPtr)
		// object UnityEngine.AndroidJavaObject.GetStatic<object>(string)
		// byte UnityEngine.AndroidJavaObject._Call<byte>(System.IntPtr,object[])
		// byte UnityEngine.AndroidJavaObject._Call<byte>(string,object[])
		// int UnityEngine.AndroidJavaObject._Call<int>(System.IntPtr,object[])
		// int UnityEngine.AndroidJavaObject._Call<int>(string,object[])
		// object UnityEngine.AndroidJavaObject._Call<object>(System.IntPtr,object[])
		// object UnityEngine.AndroidJavaObject._Call<object>(string,object[])
		// object UnityEngine.AndroidJavaObject._GetStatic<object>(System.IntPtr)
		// object UnityEngine.AndroidJavaObject._GetStatic<object>(string)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object UnityEngine.JsonUtility.FromJson<object>(string)
		// object UnityEngine.Object.FindAnyObjectByType<object>()
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion)
		// byte UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<byte>(System.IntPtr)
		// int UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<int>(System.IntPtr)
		// object UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<object>(System.IntPtr)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetFieldID<object>(System.IntPtr,string,bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<byte>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<int>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<object>(System.IntPtr,string,object[],bool)
		// string UnityEngine._AndroidJNIHelper.GetSignature<byte>(object[])
		// string UnityEngine._AndroidJNIHelper.GetSignature<int>(object[])
		// string UnityEngine._AndroidJNIHelper.GetSignature<object>(object[])
	}
}