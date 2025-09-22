using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Easy
{
    public abstract class AssetsLoader
    {
        public abstract void BeforeRestart();

        public abstract void Init(InitCompleteCallback complete);

        public abstract bool NeedForeUpdate();

        public abstract void Update();

        public abstract float GetUpdateProgress();
        
        public abstract void UnloadUnusedAssets();
        
        public abstract ISingleUnityAssetHandle<UnityEngine.Object> LoadUnityAsset(string path, Type type);

        public abstract ISingleUnityAssetHandle<T> LoadUnityAsset<T>(string path) where T : UnityEngine.Object;
        
        public abstract IMultiUnityAssetHandle LoadUnityAssetsByKey(string key, Type type);

        public abstract IMultiUnityAssetHandle LoadUnityAssetsByKey<T>(string key) where T : UnityEngine.Object;

        public abstract IMultiUnityAssetHandle LoadUnityAssetsByPath(IEnumerable<string> paths, Type type);

        public abstract IMultiUnityAssetHandle LoadUnityAssetsByPath<T>(IEnumerable<string> paths) where T : UnityEngine.Object;

        public abstract ISingleRawAssetHandle LoadRawAsset(string path);

        public abstract IMultiRawAssetHandle LoadRawAssetsByKey(string key);

        public abstract IMultiRawAssetHandle LoadRawAssetsByPath(IEnumerable<string> paths);

        public abstract List<string> KeyToAssetPaths(string key);

        public abstract void Release(IBaseAssetHandle handle);
        
    }

}