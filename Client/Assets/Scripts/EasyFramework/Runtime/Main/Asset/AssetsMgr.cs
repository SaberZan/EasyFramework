using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybridCLR;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Easy
{
    public enum AssetsMgrState
    {
        None,
        InProgress,
        Success,
        Fail,
    }


    [PreInit]
    [Update]
    [OrderIndex((int)PreInitOrderIndexEnum.AssetsMgr)]
    public class AssetsMgr : Singleton<AssetsMgr>
    {

        /// <summary>
        /// 初始化成功回调
        /// </summary>
        private InitCompleteCallback _initComplete;
        
        /// <summary>
        /// 资源加载器
        /// </summary>
        private AssetsLoader _assetsLoader;

        /// <summary>
        /// 资源管理器状态
        /// </summary>
        private AssetsMgrState state;


        public override void BeforeRestart()
        {
            state = AssetsMgrState.None;
            _assetsLoader?.BeforeRestart();
            _assetsLoader = null;
        }

        public override void Init(InitCompleteCallback complete)
        {
            state = AssetsMgrState.InProgress;
#if AA
            _assetsLoader = new Easy.AA.AAAssetsLoader();
#else
            _assetsLoader = new Easy.EasyAsset.EasyAssetsLoader();
#endif
            _assetsLoader.Init((result)=>
            {
                Debug.Log($"_assetsLoader result:{result}");
                if (result)
                {
                    LoadMetadataForAOTAssemblies();
                    LoadHotUpdateDlls();
                    state = AssetsMgrState.Success;
                }
                else
                {
                    state = AssetsMgrState.Fail;
                }
                complete?.Invoke(result);
                
            });
        }
        
        void LoadHotUpdateDlls()
        {
            Debug.Log($"LoadHotUpdateDlls");
#if UNITY_EDITOR
            AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "Assembly-CSharp");
#else
            var handle = AssetsMgr.Instance.LoadAssetsByKey<TextAsset>("HotUpdate");
            foreach (var o in handle.GetResult())
            {
                var textAsset = (TextAsset) o;
                byte[] dllBytes = textAsset.bytes;
                System.Reflection.Assembly.Load(dllBytes);
                Debug.Log($"LoadHotUpdateDlls:{textAsset.name}");
            }
            AssetsMgr.Instance.Release(handle);
#endif
        }
        
        
        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            Debug.Log($"LoadMetadataForAOTAssemblies");
#if UNITY_EDITOR

#else
            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 我们在BuildProcessors里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            var handle = AssetsMgr.Instance.LoadAssetsByKey<TextAsset>("AOT");
            foreach (var o in handle.GetResult())
            {
                var textAsset = (TextAsset) o;
                byte[] dllBytes = textAsset.bytes;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
                Debug.Log($"LoadMetadataForAOTAssembly:{textAsset.name}. ret:{err}");
            }
            AssetsMgr.Instance.Release(handle);
#endif
        }

        public AssetsMgrState GetState()
        {
            return state;
        }

        public void Update() 
        {
            _assetsLoader.Update();
        }

        public float GetUpdateProgress()
        {
            return _assetsLoader.GetUpdateProgress();
        }

        public bool NeedForeUpdate()
        {
            return _assetsLoader.NeedForeUpdate();
        }

        public void UnloadUnusedAssets()
        {
            _assetsLoader.UnloadUnusedAssets();
        }
        
        public static ISingleUnityAssetHandle<UnityEngine.Object> StaticLoadAsset(string path, Type type)
        {
            return AssetsMgr.Instance.LoadAsset(path, type);
        }
        
        public static IMultiUnityAssetHandle StaticLoadAssetsByKey(string key, Type type)
        {
            return AssetsMgr.Instance.LoadAssetsByKey(key, type);
        }

        public static IMultiUnityAssetHandle StaticLoadAssetsByPath(IEnumerable<string> paths, Type type)
        {
            return AssetsMgr.Instance.LoadAssetsByPath(paths, type);
        }

        public static ISingleRawAssetHandle StaticLoadRawAsset(string path)
        {
            return AssetsMgr.Instance.LoadRawAsset(path);
        }

        public static IMultiRawAssetHandle StaticLoadRawAssetsByKey(string key)
        {
            return AssetsMgr.Instance.LoadRawAssetsByKey(key);
        }

        public static IMultiRawAssetHandle StaticLoadRawAssetsByPath(IEnumerable<string> paths)
        {
            return AssetsMgr.Instance.LoadRawAssetsByPath(paths);
        }

        public static void StaticRelease(IBaseAssetHandle baseAssetHandle)
        {
            AssetsMgr.Instance.Release(baseAssetHandle);
        }

        public ISingleUnityAssetHandle<UnityEngine.Object> LoadAsset(string path, Type type)
        {
            return _assetsLoader.LoadUnityAssetByPath(path, type);
        }
        
        public ISingleUnityAssetHandle<T> LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            return _assetsLoader.LoadUnityAssetByPath<T>(path);
        }
        
        public IMultiUnityAssetHandle LoadAssetsByKey(string key, Type type)
        {
            return _assetsLoader.LoadUnityAssetsByKey(key, type);
        }

        public IMultiUnityAssetHandle LoadAssetsByKey<T>(string key) where T : Object
        {
            return _assetsLoader.LoadUnityAssetsByKey<T>(key);
        }

        public IMultiUnityAssetHandle LoadAssetsByPath(IEnumerable<string> paths, Type type)
        {
            return _assetsLoader.LoadUnityAssetsByPaths(paths, type);
        }

        public IMultiUnityAssetHandle LoadAssetsByPath<T>(IEnumerable<string> paths) where T : Object
        {
            return _assetsLoader.LoadUnityAssetsByPaths<T>(paths);
        }

        /// <summary>
        /// 加载原始资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ISingleRawAssetHandle LoadRawAsset(string path)
        {
            return _assetsLoader.LoadRawAsset(path);
        }

        /// <summary>
        /// 通过key加载原始资源
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IMultiRawAssetHandle LoadRawAssetsByKey(string key)
        {
            return _assetsLoader.LoadRawAssetsByKey(key);
        }

        /// <summary>
        /// 加载多原始资源
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public IMultiRawAssetHandle LoadRawAssetsByPath(IEnumerable<string> paths)
        {
            return _assetsLoader.LoadRawAssetsByPath(paths);
        }

        /// <summary>
        /// 释放句柄
        /// </summary>
        /// <param name="handle"></param>
        public void Release(IBaseAssetHandle handle)
        {
            _assetsLoader.Release(handle);
        }
    }

}