using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Easy
{
    public interface IBaseAssetHandle
    { 
        public bool IsDone();
        public void Reuse();
        public void Reset();
        public void WaitForCompletion();
    }

    public interface IBaseUnityAssetHandle : IBaseAssetHandle
    {
        public bool IsAutoRelease();
        public void SetAutoRelease(bool value);
        public int AliveCount();
    }

    public interface IUnityAssetHandle <T> : IBaseUnityAssetHandle
    {

    }
    
    public interface ISingleUnityAssetHandle<T> : IUnityAssetHandle<T> where T : UnityEngine.Object
    {
        public string GetPath();
        public T GetResult();
        public UniTask<T> GetResultAsync(Action<T> action = null);
        public T Instantiate();
        public UniTask<T> InstantiateAsync(Action<T> action = null);
        public void ReleaseInstance(T t);
    }

    public interface IMultiUnityAssetHandle : IUnityAssetHandle<UnityEngine.Object>
    {
        public List<string> GetPaths();
        public List<UnityEngine.Object> GetResult();
        public UniTask<List<UnityEngine.Object>> GetResultAsync(Action<List<UnityEngine.Object>> action = null);
        public UnityEngine.Object Instantiate(string path);
        public UniTask<UnityEngine.Object> InstantiateAsync(string path, Action<UnityEngine.Object> action = null);
        public List<UnityEngine.Object> Instantiates();
        public UniTask<List<UnityEngine.Object>> InstantiatesAsync(Action<List<UnityEngine.Object>> action = null);
        public void ReleaseInstance(UnityEngine.Object t);
        public void ReleaseInstances(List<UnityEngine.Object> t);
    }

    public interface ISingleRawAssetHandle : IBaseAssetHandle
    {
        public string GetPath();
        public byte[] GetResult();
        public UniTask<byte[]> GetResultAsync(Action<byte[]> action = null);
    }

    public interface IMultiRawAssetHandle : IBaseAssetHandle
    {
        public List<string> GetPaths();
        public List<byte[]> GetResult();
        public UniTask<List<byte[]>> GetResultAsync(Action<List<byte[]>> action = null);
    }
}
