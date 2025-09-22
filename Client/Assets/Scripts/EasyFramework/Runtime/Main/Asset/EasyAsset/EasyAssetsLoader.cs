using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Easy.EasyAsset
{

    /// <summary>
    /// 资源管理类
    /// 管理资源加载和释放；
    /// 并且管理ab包， 及其ab包内加载资源。
    /// </summary>
    public class EasyAssetsLoader : AssetsLoader
    {
        /// <summary>
        /// 所有资源信息类对象l
        /// </summary>
        public Catalogs catalogs;

        /// <summary>
        /// ab包缓冲字典
        /// </summary>
        private Dictionary<int, EasyAssetBundle> _assetBundlesCache = new Dictionary<int, EasyAssetBundle>();

        /// <summary>
        /// ab包的引用计数
        /// </summary>
        private Dictionary<int, int> _assetBundlesReference = new Dictionary<int, int>();
        
        /// <summary>
        /// 持有的handle
        /// </summary>
        private List<IBaseAssetHandle> _handles = new List<IBaseAssetHandle>();

        /// <summary>
        /// handle的缓存池
        /// </summary>
        private Dictionary<Type, List<IBaseAssetHandle>> _handleCache = new Dictionary<Type, List<IBaseAssetHandle>>();
        
        /// <summary>
        /// 初始化成功回调
        /// </summary>
        private InitCompleteCallback _initComplete;

        /// <summary>
        /// 进度
        /// </summary>
        private float _progress = 0;

        /// <summary>
        /// 热更下载,必须下载完必要包,才会生效
        /// </summary>
        private DownloadCtrl _abDownloadCtrl;

#if UNITY_ANDROID
        public PlayAssetPackCtrl _playAssetPackCtrl;

        public PlayAssetPackCtrl playAssetPackCtrl => _playAssetPackCtrl;
#endif

        /// <summary>
        /// 重新启动接口
        /// </summary>
        public override void BeforeRestart()
        {
            foreach (var kv in _assetBundlesCache)
            {
                kv.Value.Unload(true);
            }
            _abDownloadCtrl.Reset();
        }

        /// <summary>
        /// 初始化接口
        /// </summary>
        /// <param name="complete"></param>
        public override void Init(InitCompleteCallback complete)
        {
#if UNITY_ANDROID
            _playAssetPackCtrl = new PlayAssetPackCtrl();
#endif

            EasyAssetConfig easyAssetConfig = EasyFrameworkMain.Instance.config.GetEasyConfig<EasyAssetConfig>();
            BaseUnityAssetHandle.instanceOverTime = easyAssetConfig.instanceOverTime;
#if UNITY_EDITOR && !AB_LOAD_BUNDLE
            string localVersion = Application.persistentDataPath + "/editorCatalogs.txt";
            catalogs = FileMgr.Instance.GetTargetClassObject<Catalogs>(localVersion);
            catalogs.Init();
            _progress = 1;
            complete.Invoke(true);
#else
            List<Action<InitCompleteCallback>> process = new List<Action<InitCompleteCallback>>();
            process.Add(CopyStreamingAssetsToPersistent);
            process.Add(CheckDownload);
            process.Add(LoadLocalCatalogs);
            int index = 0;
            _initComplete = (result)=>
            {
                if(result)
                {
                    ++index;
                    _progress += 0.33f;
                    if (index == process.Count)
                    {
                        catalogs.Init();
                        complete.Invoke(true);
                    }
                    else
                    {
                        process[index](_initComplete);
                    }
                }
                else
                {
                    complete.Invoke(false);
                }
            };
            process[index](_initComplete);
#endif
        }

        /// <summary>
        /// 检测下载
        /// </summary>
        /// <param name="complete"></param>
        private void CheckDownload(InitCompleteCallback complete)
        {
            _abDownloadCtrl = new DownloadCtrl();
            _abDownloadCtrl.CheckUpdate(this, complete);
        }

        /// <summary>
        /// 加载本地资源详情
        /// </summary>
        /// <param name="complete"></param>
        private void LoadLocalCatalogs(InitCompleteCallback complete)
        {
            catalogs = FileMgr.Instance.GetTargetClassObject<Catalogs>(Const.localVersionPath);
            _progress = 1;
            complete.Invoke(true);
        }

        /// <summary>
        /// 拷贝到可读写路径
        /// </summary>
        /// <param name="complete"></param>
        private void CopyStreamingAssetsToPersistent(InitCompleteCallback complete)
        {
#if UNITY_EDITOR && !AB_LOAD_BUNDLE
#else
            if (!File.Exists(Const.originalVersionPath))
                complete.Invoke(false);

            Catalogs originTable = FileMgr.Instance.GetTargetClassObject<Catalogs>(Const.originalVersionPath, true);
            Catalogs localTable = FileMgr.Instance.GetTargetClassObject<Catalogs>(Const.localVersionPath, false);
            
            // 发现可读写内有版本信息
            if (localTable == null || localTable.versionCode < originTable.versionCode || localTable.version < originTable.version)
            {
                if (!Directory.Exists(Const.localAssetBundleFolder))
                    Directory.CreateDirectory(Const.localAssetBundleFolder);

                // 文件夹内文件拷贝到可读写目录
                List<string> allFiles = new List<string>(originTable.allEasyAssetBundleInfos.Count);
                for (int i = 0, count = originTable.allEasyAssetBundleInfos.Count; i < count; i++)
                {
                    allFiles.Add(originTable.allEasyAssetBundleInfos[i].md5);
                }
                
                for(int i = 0, count = allFiles.Count; i < count; ++i)
                {
                    string originFile = Const.originAssetBundleFolder + allFiles[i];
                    string localFile = Const.localAssetBundleFolder + allFiles[i];
                    if (!File.Exists(originFile))
                    {
                        continue;
                    }
#if UNITY_ANDROID
                    byte[] content_buffer = FileMgr.Instance.LoadStreamingAssetFileSync(originFile);
                    File.WriteAllBytes(localFile, content_buffer);
#else
                    File.Copy(originFile, localFile, true);
#endif
                }

                FileMgr.Instance.SetTargetClassObjectToFile<Catalogs>(Const.localVersionPath, originTable);
            }
#endif
            complete.Invoke(true);
        }
        
        /// <summary>
        /// 定时刷新
        /// </summary>    
        public override void Update()
        {
            for(int i = _handles.Count - 1; i >= 0; --i)
            {
                if(_handles[i] is BaseUnityAssetHandle handle)
                {
                    if (TryReleaseAssetHandle(handle))
                    {
                        PutHandleToCache(_handles[i]);
                        _handles.Remove(_handles[i]);
                    }   
                }
            }
        }

        private EasyAssetBundle GetEasyAssetBundle(string assetName)
        {
            EasyAssetInfo easyAssetInfo = catalogs.GetEasyAssetInfoByAsset(assetName);
            if (easyAssetInfo.asset.Equals(assetName, StringComparison.OrdinalIgnoreCase))
            {
                List<int> needABIndexes = easyAssetInfo.needABIndexes;
                for(int j = 0; j < needABIndexes.Count; ++j)
                {
                    LoadAssetBundleByIndex(needABIndexes[j]);
                }
                if (_assetBundlesCache.ContainsKey(easyAssetInfo.abIndex))
                {
                    return _assetBundlesCache[easyAssetInfo.abIndex];
                }
            }
            return null;
        }
        
        /// <summary>
        /// 加载ab包
        /// </summary>
        /// <param name="index">ab包序号</param>
        /// <param name="action">回调方法</param>
        private bool LoadAssetBundleByIndex(int index, Action action = null)
        {
            if (_assetBundlesCache.ContainsKey(index))
                return false;
            EasyAssetBundle assetBundle = null;
            EasyAssetBundleInfo abInfo = catalogs.allEasyAssetBundleInfos[index];
            if( abInfo.easyAssetBundleType == EasyAssetBundleType.UnityAssetBundle)
            {
                assetBundle = new UnityAssetBundle(this, index);
            }
            else if(abInfo.easyAssetBundleType == EasyAssetBundleType.RawAssetBundle)
            {
                assetBundle = new RawAssetBundle(this, index);
            }
            if(assetBundle == null)
                return false;
            _assetBundlesCache.Add(index, assetBundle);
            action?.Invoke();
            return true;
        }

        #region  download
        /// <summary>
        /// 检测某个资源包是否全都准备好
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public bool CheckPackageCompleted(string packageName)
        {
#if UNITY_EDITOR && !AB_LOAD_BUNDLE
            return true;
#else
            for (int i = 0, abCount = catalogs.allEasyAssetBundleInfos.Count; i < abCount; i++)
            {
                if(catalogs.allEasyAssetBundleInfos[i].packages.Contains(packageName) && !CheckEasyAssetBundleCompleted(i))
                {
                    return false;
                }
            }
            return true;
#endif
        }

        /// <summary>
        /// 开始下载资源包
        /// </summary>
        /// <param name="packageName"></param>
        public void StartDownloadPackage(string packageName)
        {
#if UNITY_EDITOR && !AB_LOAD_BUNDLE

#else
            for (int i = 0, abCount = catalogs.allEasyAssetBundleInfos.Count; i < abCount; i++)
            {
                if(catalogs.allEasyAssetBundleInfos[i].packages.Contains(packageName) && !CheckEasyAssetBundleCompleted(i))
                {
                    StartDownloadEasyAssetBundle(i);
                }
            }
#endif
        }

        /// <summary>
        /// 检测ab包是否下载好
        /// </summary>
        /// <param name="abIndex"></param>
        /// <returns></returns>
        public bool CheckEasyAssetBundleCompleted(int abIndex)
        {
#if UNITY_EDITOR && !AB_LOAD_BUNDLE
            return true;
#else
            EasyAssetBundleInfo easyAssetBundleInfo = catalogs.allEasyAssetBundleInfos[abIndex];
#if UNITY_ANDROID
            FileInfo fileInfo = new FileInfo(Const.localAssetBundleFolder + easyAssetBundleInfo.md5);
            if(fileInfo.Exists && fileInfo.Length == easyAssetBundleInfo.size)
            {
                return true;
            }
            return _playAssetPackCtrl.IsAssetBundleDownloaded(easyAssetBundleInfo.md5);
#else
            FileInfo fileInfo = new FileInfo(Const.localAssetBundleFolder + easyAssetBundleInfo.md5);
            return fileInfo.Exists && fileInfo.Length == easyAssetBundleInfo.size;
#endif

#endif
        }

        /// <summary>
        /// 开始下载ab包
        /// </summary>
        /// <param name="abIndex"></param>
        public void StartDownloadEasyAssetBundle(int abIndex)
        {
#if UNITY_EDITOR && !AB_LOAD_BUNDLE

#else
            EasyAssetBundleInfo abInfo = catalogs.allEasyAssetBundleInfos[abIndex];
            _abDownloadCtrl.StartDownload(abInfo.md5);
#endif
        }
        
        /// <summary>
        /// 是否下载失败
        /// </summary>
        /// <returns></returns>
        public bool IsDownLoadFail()
        {
#if UNITY_EDITOR && !AB_LOAD_BUNDLE
            return false;
#else
            return _abDownloadCtrl.IsDownLoadFail();
#endif
        }

        #endregion

        #region LoadAsset
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private UnityEngine.Object LoadUnityAssetAtPath(string assetName, Type type)
        {
            if (GetEasyAssetBundle(assetName) is UnityAssetBundle unityAssetBundle)
            {
                return unityAssetBundle.LoadAsset(assetName, type);
            }
            return null;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T LoadUnityAssetAtPath<T>(string assetName) where T : UnityEngine.Object
        {
            return (T)LoadUnityAssetAtPath(assetName, typeof(T));
        }
        
        /// <summary>
        /// 内部加载资源接口
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private UnityEngine.Object LoadUnityAssetInternal(string path, Type type)
        {
            UnityEngine.Object res = null;
#if UNITY_EDITOR && !AB_LOAD_BUNDLE
            res = AssetDatabase.LoadAssetAtPath(path, type);
#else
            res = LoadUnityAssetAtPath(path, type);
#endif
            return res;
        }
        

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private byte[] LoadRawAssetAtPath(string assetName)
        {
            if (GetEasyAssetBundle(assetName) is RawAssetBundle rawAssetBundle)
            {
                return rawAssetBundle.LoadAsset(assetName);
            }
            return null;
        }

        /// <summary>
        /// 内部加载资源接口
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadUnityAssetInternal<T>(string path) where T : UnityEngine.Object
        {
            T res = null;
#if UNITY_EDITOR && !AB_LOAD_BUNDLE
            res = AssetDatabase.LoadAssetAtPath<T>(path);
#else
            res = LoadUnityAssetAtPath<T>(path);
#endif
            return res;
        }

        /// <summary>
        /// 内部加载资源接口
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public byte[] LoadRawAssetInternal(string path)
        {
            byte[] res = null;
#if UNITY_EDITOR && !AB_LOAD_BUNDLE
            res = File.ReadAllBytes(path);
#else
            res = LoadRawAssetAtPath(path);
#endif
            return res;
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
            handle.loader = this;
            handle.path = path;
            handle.dependAB = catalogs.GetDependEasyAssetBundleIndexesByAssetName(path);
            _handles.Add(handle);
            RetainHandle(handle);
            handle.Start();
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
            handle.loader = this;
            handle.path = path;
            handle.dependAB = catalogs.GetDependEasyAssetBundleIndexesByAssetName(path);
            _handles.Add(handle);
            RetainHandle(handle);
            handle.Start();
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
            handle.loader = this;
            List<int> depends = new List<int>(); 
            foreach (var dicKv in catalogs.allActiveKeyToAssetPaths)
            {
                if(dicKv.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    handle.paths.AddRange(dicKv.Value);
                    break;
                }           
            }
            foreach (var path in handle.paths)
            {
                depends.AddRange(catalogs.GetDependEasyAssetBundleIndexesByAssetName(path));
            }
            handle.dependAB = depends.Distinct<int>().ToList();
            _handles.Add(handle);
            RetainHandle(handle);
            handle.Start();
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
            return LoadUnityAssetsByKey(key, typeof(T));
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
            handle.loader = this;
            List<int> depends = new List<int>();
            foreach (string path in paths)
            {
                if (type == null || catalogs.GetTypeByAssetName(path).IsAssignableFrom(type))
                {
                    handle.paths.Add(path);
                    depends.AddRange(catalogs.GetDependEasyAssetBundleIndexesByAssetName(path));
                }
            }
            handle.dependAB = depends.Distinct<int>().ToList();
            _handles.Add(handle);
            RetainHandle(handle);
            handle.Start();
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

        /// <summary>
        /// 加载原始资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override ISingleRawAssetHandle LoadRawAsset(string path)
        {
            SingleRawAssetHandle handle = GetHandleFromCache<SingleRawAssetHandle>();
            handle.loader = this;
            handle.path = path;
            handle.dependAB.AddRange(catalogs.GetDependEasyAssetBundleIndexesByAssetName(path));
            _handles.Add(handle);
            RetainHandle(handle);
            handle.Start();
            return handle;
        }

        /// <summary>
        /// 通过key加载原始资源
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IMultiRawAssetHandle LoadRawAssetsByKey(string key)
        {
            MultiRawAssetHandle handle = GetHandleFromCache<MultiRawAssetHandle>();
            handle.loader = this;
            List<int> depends = new List<int>();
            foreach (var dicKv in catalogs.allActiveKeyToAssetPaths)
            {
                if(dicKv.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    handle.paths.AddRange(dicKv.Value);
                    break;
                }           
            }
            foreach (var path in handle.paths)
            {
                depends.AddRange(catalogs.GetDependEasyAssetBundleIndexesByAssetName(path));
            }
            handle.dependAB = depends.Distinct<int>().ToList();
            _handles.Add(handle);
            RetainHandle(handle);
            handle.Start();
            return handle;
        }

        /// <summary>
        /// 加载多原始资源
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public override IMultiRawAssetHandle LoadRawAssetsByPath(IEnumerable<string> paths)
        {
            MultiRawAssetHandle handle = GetHandleFromCache<MultiRawAssetHandle>();
            handle.loader = this;
            List<int> depends = new List<int>(); 
            foreach (string path in paths)
            {
                handle.paths.Add(path);
                depends.AddRange(catalogs.GetDependEasyAssetBundleIndexesByAssetName(path));
            }
            handle.dependAB = depends.Distinct<int>().ToList();
            _handles.Add(handle);
            RetainHandle(handle);
            handle.Start();
            return handle;
        }

        /// <summary>
        /// 通过key转资源路径
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override List<string> KeyToAssetPaths(string key)
        {
            foreach (var assetInfoKv in catalogs.allActiveKeyToAssetPaths)
            {
                if (assetInfoKv.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return assetInfoKv.Value;
                }
            }
            
            return null;
        }

        #endregion

        #region handle
        /// <summary>
        /// 从缓存中拿handle句柄
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetHandleFromCache<T>() where T : IBaseAssetHandle, new()
        {
            Type type = typeof(T);
            if (_handleCache.ContainsKey(type) && _handleCache[type].Count > 0)
            {
                T handle = (T)_handleCache[type].Pop();
                handle.Reuse();
                return handle;
            }
            return new T();
        }
        
        /// <summary>
        /// 从缓存中拿handle句柄
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IBaseAssetHandle GetHandleFromCache(Type type)
        {
            if (_handleCache.ContainsKey(type) && _handleCache[type].Count > 0)
            {
                IBaseAssetHandle handle = _handleCache[type].Pop();
                handle.Reuse();
                return handle;
            }
            return (IBaseAssetHandle) System.Activator.CreateInstance(type);
        }
        
        /// <summary>
        /// 将无用句柄缓存
        /// </summary>
        /// <param name="handle"></param>
        private void PutHandleToCache(IBaseAssetHandle handle)
        {
            Type type = handle.GetType();
            if (!_handleCache.ContainsKey(type))
            {
                _handleCache.Add(type, new List<IBaseAssetHandle>());
            }
            handle.Reset();
            _handleCache[type].Push(handle);
        }

        /// <summary>
        /// 对ab包添加引用计数,不针对Load出来的资源进行引用计数，当ab包引用计数为0时，释放ab包和Load出来的资源
        /// </summary>
        private void RetainHandle(IBaseAssetHandle handle)
        {
            if (handle is BaseAssetHandle assetHandle)
            {
                foreach (var abName in assetHandle.dependAB)
                {
                    if (!_assetBundlesReference.ContainsKey(abName))
                        _assetBundlesReference.Add(abName, 0);
                    ++_assetBundlesReference[abName];
                }
            }
        }
        
        /// <summary>
        /// 尝试释放handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        private bool TryReleaseAssetHandle(BaseAssetHandle handle)
        {
            if(handle is BaseUnityAssetHandle baseUnityAssetHandle && baseUnityAssetHandle.AliveCount() > 0)
            {
                EasyLogger.Log("EasyFrameWork","实例还没被销毁,不应删除句柄");
                return false;
            }
            foreach (var abName in handle.dependAB)
            {
                if (!_assetBundlesReference.ContainsKey(abName))
                    _assetBundlesReference.Add(abName, 0);
                --_assetBundlesReference[abName];
            }
            return true;
        }

        /// <summary>
        /// 对ab包减少引用计数,不针对Load出来的资源进行引用计数，当ab包引用计数为0时，释放ab包和Load出来的资源
        /// </summary>
        private bool ReleaseHandle(IBaseAssetHandle handle)
        {
            if (handle is BaseAssetHandle assetHandle)
            {
                if (TryReleaseAssetHandle(assetHandle))
                {
                    PutHandleToCache(handle);
                    _handles.Remove(handle);
                    return true;
                }
            }
            else
            {
                PutHandleToCache(handle);
                _handles.Remove(handle);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 是否句柄
        /// </summary>
        /// <param name="handle"></param>
        public override void Release(IBaseAssetHandle handle)
        {
            ReleaseHandle(handle);                
        }

        #endregion

        /// <summary>
        /// 卸载未使用资源
        /// </summary>
        public override void UnloadUnusedAssets()
        {
            foreach (var kv in _assetBundlesReference)
            {
                if (kv.Value == 0)
                {
                    _assetBundlesCache[kv.Key].Unload(true);
                    _assetBundlesCache.Remove(kv.Key);
                }
            }
        }
        
        /// <summary>
        /// 获得更新进度
        /// </summary>
        /// <returns></returns>
        public override float GetUpdateProgress()
        {
            return _progress;
        }

        /// <summary>
        /// 是否强更新
        /// </summary>
        /// <returns></returns>
        public override bool NeedForeUpdate()
        {
            return _abDownloadCtrl?.NeedForeUpdate() == true;
        }

    }
}