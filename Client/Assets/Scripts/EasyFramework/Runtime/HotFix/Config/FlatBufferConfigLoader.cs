using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;

namespace Easy
{
    public class FlatBufferConfigLoader : ConfigLoader
    {

        private readonly Dictionary<string, Type> _needLoadConfig = new Dictionary<string, Type>();

        private readonly Dictionary<string, object> _allConfigs = new Dictionary<string, object>();

        private InitCompleteCallback _loadConfigSuccess;

        private int _num;
        
        public override void BeforeRestart()
        {
            
        }
        
        public override void Init(InitCompleteCallback complete)
        {
            _loadConfigSuccess = complete;
            List<Type> types = EasyFrameworkMain.Instance.GetTypes();
            foreach (var t in types)
            {
                if (t.IsAbstract)
                {
                    continue;
                }
                if (t.IsInterface)
                {
                    continue;
                }

                if (!t.IsDefined(typeof(ConfigAttribute)))
                {
                    continue;
                }

                ConfigAttribute attribute = t.GetCustomAttribute<ConfigAttribute>();
                string configName = attribute.configName;

                _needLoadConfig.Add(configName, t);
            }

            if (_needLoadConfig.Count <= 0)
            {
                _loadConfigSuccess(true);
            }

            ConfigEasyConfig configEasyConfig = EasyFrameworkMain.Instance.config.GetEasyConfig<ConfigEasyConfig>();
            string configPath = configEasyConfig.GetConfigPath();
            if(!configPath.EndsWith("/"))
            {
                configPath += "/";
            }
            foreach (var kv in _needLoadConfig)
            {
                LoadConfigCacheAsync(configPath + kv.Key + ".bin", kv.Value, LoadConfigCallBack);
            }

        }

        private void LoadConfigCallBack(object data)
        {
            ++_num;
            if (_num == _needLoadConfig.Count)
            {
                _loadConfigSuccess(true);
            }
        }


        /// <summary>
        /// 获取配置并持有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// 
        public override object LoadConfigCache(string path, Type type) 
        {
            var handle = Easy.AssetsMgr.Instance.LoadRawAsset(path);
            byte[] bytes = handle.GetResult();
            MethodInfo methodInfo = type.GetMethod("CreateConfig", BindingFlags.Static | BindingFlags.Public);
            object configData = methodInfo.Invoke(null, new object[]{ new Google.FlatBuffers.ByteBuffer(bytes)});
            _allConfigs.Add(type.Name, configData);
            Easy.AssetsMgr.Instance.Release(handle);
            return configData;
        }
        
        /// <summary>
        /// 异步获取配置并持有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// 
        public override async void LoadConfigCacheAsync(string path, Type type, Action<object> callback) 
        {
            var handle = Easy.AssetsMgr.Instance.LoadRawAsset(path);
            byte[] bytes = await handle.GetResultAsync();
            MethodInfo methodInfo = type.GetMethod("CreateConfig", BindingFlags.Static | BindingFlags.Public);
            object configData = methodInfo.Invoke(null, new object[]{ new Google.FlatBuffers.ByteBuffer(bytes)});
            _allConfigs.Add(type.Name, configData);
            Easy.AssetsMgr.Instance.Release(handle);
            callback(configData);
        }
        
        /// <summary>
        /// 获取配置并持有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override void LoadConfigCache<T>(string path) 
        {
            Type type = typeof(T);
            var handle = Easy.AssetsMgr.Instance.LoadRawAsset(path);
            byte[] bytes = handle.GetResult();
            MethodInfo methodInfo = type.GetMethod("CreateConfig", BindingFlags.Static | BindingFlags.Public);
            T configData = (T)methodInfo.Invoke(null, new object[]{ new Google.FlatBuffers.ByteBuffer(bytes) });
            _allConfigs.Add(type.Name, configData);
            Easy.AssetsMgr.Instance.Release(handle);
        }
        
        /// <summary>
        /// 异步获取配置并持有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override async void LoadConfigCacheAsync<T>(string path, Action<T> callback) 
        {
            Type type = typeof(T);
            var handle = Easy.AssetsMgr.Instance.LoadRawAsset(path);
            byte[] bytes = await handle.GetResultAsync();
            MethodInfo methodInfo = type.GetMethod("CreateConfig", BindingFlags.Static | BindingFlags.Public);
            T configData = (T)methodInfo.Invoke(null, new object[]{ new Google.FlatBuffers.ByteBuffer(bytes)});
            _allConfigs.Add(type.Name, configData);
            callback(configData);
            Easy.AssetsMgr.Instance.Release(handle);
        }

        
        
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// 
        public override object LoadConfigUnCache(string path, Type type) 
        {
            var handle = Easy.AssetsMgr.Instance.LoadRawAsset(path);
            byte[] bytes = handle.GetResult();
            MethodInfo methodInfo = type.GetMethod("CreateConfig", BindingFlags.Static | BindingFlags.Public);
            object configData = methodInfo.Invoke(null, new object[]{ new Google.FlatBuffers.ByteBuffer(bytes)});
            Easy.AssetsMgr.Instance.Release(handle);
            return configData;
        }
        
        /// <summary>
        /// 异步获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// 
        public override async void LoadConfigUnCacheAsync(string path, Type type, Action<object> callback) 
        {
            var handle = Easy.AssetsMgr.Instance.LoadRawAsset(path);
            byte[] bytes = await handle.GetResultAsync();
            MethodInfo methodInfo = type.GetMethod("CreateConfig", BindingFlags.Static | BindingFlags.Public);
            object configData = methodInfo.Invoke(null, new object[]{ new Google.FlatBuffers.ByteBuffer(bytes)});
            callback(configData);
            Easy.AssetsMgr.Instance.Release(handle);
        }
        
        
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T LoadConfigUnCache<T>(string path)
        {
            Type type = typeof(T);
            var handle = Easy.AssetsMgr.Instance.LoadRawAsset(path);
            byte[] bytes = handle.GetResult();
            MethodInfo methodInfo = type.GetMethod("CreateConfig", BindingFlags.Static | BindingFlags.Public);
            T configData = (T)methodInfo.Invoke(null, new object[]{ new Google.FlatBuffers.ByteBuffer(bytes)});
            Easy.AssetsMgr.Instance.Release(handle);
            return configData;
        }
        
        /// <summary>
        /// 异步获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override async void LoadConfigUnCacheAsync<T>(string path, Action<T> callback)
        {
            Type type = typeof(T);
            var handle = Easy.AssetsMgr.Instance.LoadRawAsset(path);
            byte[] bytes = await handle.GetResultAsync();
            MethodInfo methodInfo = type.GetMethod("CreateConfig", BindingFlags.Static | BindingFlags.Public);
            T configData = (T)methodInfo.Invoke(null, new object[]{ new Google.FlatBuffers.ByteBuffer(bytes)});
            callback(configData);
            Easy.AssetsMgr.Instance.Release(handle);
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T Get<T>()
        {
            if (_allConfigs.ContainsKey(typeof(T).Name))
            {
                return (T)_allConfigs[typeof(T).Name];
            }
            return default(T);
        }
    }
}
