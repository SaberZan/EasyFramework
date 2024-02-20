using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace Easy.AA
{
    /// <summary>
    /// AA加载器
    /// </summary>
    public class AAAssetsLoader : AssetsLoader
    {

        private InitCompleteCallback _initComplete;
        
        /// <summary>
        /// 热更新进度
        /// </summary>
        private float _updateProgress = 0;
        
        /// <summary>
        /// 持有的handle
        /// </summary>
        private List<IBaseAssetHandle> _handles = new List<IBaseAssetHandle>();

        /// <summary>
        /// handle的缓存池
        /// </summary>
        private Dictionary<Type, List<IBaseAssetHandle>> _handleCache = new Dictionary<Type, List<IBaseAssetHandle>>();

        public override void BeforeRestart()
        {

        }

        public override void Init(InitCompleteCallback complete)
        {
            _initComplete = complete;
            if (EasyFrameworkMain.Instance.config.GetEasyConfig<Easy.EasyAsset.EasyAssetConfig>().hotUpdate)
            {
                CheckUpdate();
            }
            else
            {
                _initComplete.Invoke(true);
            }
        }

        /// <summary>
        /// 检测更新
        /// </summary>
        /// <returns></returns>
        private async void CheckUpdate()
        {
            await Addressables.InitializeAsync().Task;

            var checkUpdateHandle = Addressables.CheckForCatalogUpdates(false);
            await checkUpdateHandle.Task;
            if (checkUpdateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var catalogs = checkUpdateHandle.Result;
                if (catalogs.Count > 0)
                {
                    EasyLogger.Log("EasyFrameWork", "Need Update Catalogs Count = " + catalogs.Count);
                    CheckUpdateAssets(catalogs);
                }
                else
                {
                    EasyLogger.Log("EasyFrameWork", "No Need Update");

                    await Task.Delay(100);
                    _initComplete.Invoke(true);
                }
            }
            else
            {
                _initComplete.Invoke(false);
            }
            Addressables.Release(checkUpdateHandle);
        }

        /// <summary>
        /// 检测更新资源
        /// </summary>
        /// <param name="catalogs"></param>
        /// <returns></returns>
        private async void CheckUpdateAssets(List<string> catalogs)
        {
            var updateAssetsHandle = Addressables.UpdateCatalogs(catalogs, false);
            await updateAssetsHandle.Task;
            if (updateAssetsHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var resourceLocators = updateAssetsHandle.Result;
                if (resourceLocators.Count > 0)
                {
                    UpdateAssets(resourceLocators);
                }
                else
                {
                    await Task.Delay(100);
                    _initComplete.Invoke(true);
                }
            }
            else
            {
                await Task.Delay(100);
                _initComplete.Invoke(false);
            }
            Addressables.Release(updateAssetsHandle);
        }

        /// <summary>
        /// 更新资源
        /// </summary>
        /// <param name="resourceLocators"></param>
        /// <returns></returns>
        private async void UpdateAssets(IList<IResourceLocator> resourceLocators)
        {
            int allCount = resourceLocators.Count;
            int count = 0;
            foreach (var resourceLocator in resourceLocators)
            {
                var downloadHandle = Addressables.DownloadDependenciesAsync(resourceLocator.Keys, Addressables.MergeMode.Union);
                await downloadHandle.Task;
                if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(downloadHandle);
                    ++count;
                }
            }
            if (allCount == count)
            {
                await Task.Delay(100);
                _initComplete.Invoke(true);
            }
            else
            {
                await Task.Delay(100);
                _initComplete.Invoke(false);
            }
        }

        /// <summary>
        /// 定时刷新
        /// </summary>        
        public override void Update()
        {

        }
        
        /// <summary>
        /// 缓存中拿句柄
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetHandleFromCache<T>() where T : IBaseAssetHandle, new()
        {
            Type type = typeof(T);
            if (_handleCache.ContainsKey(type) && _handleCache[type].Count > 0)
            {
                return (T)_handleCache[type].Pop();
            }
            return new T();
        }
        
        /// <summary>
        /// 缓存中拿句柄
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IBaseAssetHandle GetHandleFromCache(Type type)
        {
            if (_handleCache.ContainsKey(type) && _handleCache[type].Count > 0)
            {
                return _handleCache[type].Pop();
            }
            return (IBaseAssetHandle) System.Activator.CreateInstance(type);
        }
        
        /// <summary>
        /// 回收句柄
        /// </summary>
        /// <param name="handle"></param>
        private void PutHandleToCache(IBaseAssetHandle handle)
        {
            Type type = handle.GetType();
            if (!_handleCache.ContainsKey(type))
            {
                _handleCache.Add(type, new List<IBaseAssetHandle>());
            }
            _handleCache[type].Push(handle);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public override void UnloadUnusedAssets()
        {
        }
        
        /// <summary>
        /// 加载单资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override ISingleUnityAssetHandle<UnityEngine.Object> LoadUnityAsset(string path, Type type)
        {
            SingleUnityAssetHandle<UnityEngine.Object> handle = GetHandleFromCache<SingleUnityAssetHandle<UnityEngine.Object>>();
            AsyncOperationHandle<UnityEngine.Object> asyncOperationHandle = Addressables.LoadAssetAsync<UnityEngine.Object>(path);
            handle.path = path;
            handle.result = asyncOperationHandle;
            _handles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 加载单资源
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override ISingleUnityAssetHandle<T> LoadUnityAsset<T>(string path)
        {
            SingleUnityAssetHandle<T> handle = GetHandleFromCache<SingleUnityAssetHandle<T>>();
            AsyncOperationHandle<T> asyncOperationHandle = Addressables.LoadAssetAsync<T>(path);
            handle.path = path;
            handle.result = asyncOperationHandle;
            _handles.Add(handle);
            return handle;
        }
        
        /// <summary>
        /// 加载多资源
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override IMultiUnityAssetHandle LoadUnityAssetsByKey(string key, Type type)
        {
            MultiUnityAssetHandle handle = GetHandleFromCache<MultiUnityAssetHandle>();
            AsyncOperationHandle<IList<IResourceLocation>> asyncOperationHandle1 = Addressables.LoadResourceLocationsAsync(key, null);
            AsyncOperationHandle<IList<UnityEngine.Object>> asyncOperationHandle2 = Addressables.LoadAssetsAsync<UnityEngine.Object>(key, null);
            handle.paths.Clear();
            handle.other = asyncOperationHandle1;
            handle.result = asyncOperationHandle2;
            _handles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 加载多资源
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override IMultiUnityAssetHandle LoadUnityAssetsByKey<T>(string key)
        {
            return LoadUnityAssetsByKey(key,typeof(T));
        }

        /// <summary>
        /// 加载多资源
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="type"></param>
        /// <returns></returns>        
        public override IMultiUnityAssetHandle LoadUnityAssetsByPath(IEnumerable<string> paths, Type type)
        {
            MultiUnityAssetHandle handle = GetHandleFromCache<MultiUnityAssetHandle>();
            handle.paths = paths.ToList();
            AsyncOperationHandle<IList<UnityEngine.Object>> asyncOperationHandle = Addressables.LoadAssetsAsync<UnityEngine.Object>(paths, null);
            handle.other = default(AsyncOperationHandle);
            handle.result = asyncOperationHandle;
            _handles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 加载多资源
        /// </summary>
        /// <param name="paths"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override IMultiUnityAssetHandle LoadUnityAssetsByPath<T>(IEnumerable<string> paths)
        {
            return LoadUnityAssetsByPath(paths, typeof(T));
        }

        public override ISingleRawAssetHandle LoadRawAsset(string path)
        {
            SingleRawAssetHandle handle = GetHandleFromCache<SingleRawAssetHandle>();
            AsyncOperationHandle<TextAsset> asyncOperationHandle = Addressables.LoadAssetAsync<TextAsset>(path);
            handle.path = path;
            handle.other = default(AsyncOperationHandle);
            handle.result = asyncOperationHandle;
            _handles.Add(handle);
            return handle;
        }

        public override IMultiRawAssetHandle LoadRawAssetsByKey(string key)
        {
            MultiRawAssetHandle handle = GetHandleFromCache<MultiRawAssetHandle>();
            AsyncOperationHandle<IList<IResourceLocation>> asyncOperationHandle1 = Addressables.LoadResourceLocationsAsync(key, null);
            AsyncOperationHandle<IList<TextAsset>> asyncOperationHandle2 = Addressables.LoadAssetsAsync<TextAsset>(key, null);
            handle.other = asyncOperationHandle1;
            handle.result = asyncOperationHandle2;
            _handles.Add(handle);
            return handle;
        }

        public override IMultiRawAssetHandle LoadRawAssetsByPath(IEnumerable<string> paths)
        {
            MultiRawAssetHandle handle = GetHandleFromCache<MultiRawAssetHandle>();
            handle.paths = paths.ToList();
            AsyncOperationHandle<IList<UnityEngine.TextAsset>> asyncOperationHandle = Addressables.LoadAssetsAsync<UnityEngine.TextAsset>(paths, null);
            handle.other = default(AsyncOperationHandle);
            handle.result = asyncOperationHandle;
            _handles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="handle"></param>
        public override void Release(IBaseAssetHandle handle)
        {
            if (handle is BaseUnityAssetHandle assetHandle)
            {
                if (assetHandle.AliveCount() > 0)
                {
                    EasyLogger.Log("EasyFrameWork","实例还没被销毁,不应删除句柄");
                }

                Addressables.Release(assetHandle.result);
                if (assetHandle.other.IsValid())
                {
                    Addressables.Release(assetHandle.other);
                }
                PutHandleToCache(handle);
                _handles.Remove(handle);
            }
        }
        
        /// <summary>
        /// 获取下载进度
        /// </summary>
        /// <returns></returns>
        public override float GetUpdateProgress()
        {
            return _updateProgress;
        }

        /// <summary>
        /// 是否强更新
        /// </summary>
        /// <returns></returns>
        public override bool NeedForeUpdate()
        {
            return false;
        }


    }

}