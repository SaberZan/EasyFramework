using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;

namespace Easy
{


    /// <summary>
    /// 数据层
    /// </summary>
    [NormalInit]
    [OrderIndex((int)NormalInitOrderIndexEnum.ConfigMgr)]
    public class ConfigMgr : Singleton<ConfigMgr>
    {
        private ConfigLoader _configLoader;
        
        public override void BeforeRestart()
        {

        }
        
        public override void Init(InitCompleteCallback complete)
        {
             ConfigEasyConfig configEasyConfig = EasyFrameworkMain.Instance.config.GetEasyConfig<ConfigEasyConfig>();
             switch (configEasyConfig.loaderType)
             {
                case ConfigLoaderType.JSON:
                    _configLoader = new JsonConfigLoader();
                    break;
                case ConfigLoaderType.FLatBuffer:
                    _configLoader = new FlatBufferConfigLoader();
                    break;
                default:
                    _configLoader = new JsonConfigLoader();
                    break;
             }
            _configLoader.Init(complete);
        }
        
        /// <summary>
        /// 获取配置并持有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// 
        public object LoadConfigCache(string path, Type type)
        {
            return _configLoader.LoadConfigCache(path, type);
        }
        
        /// <summary>
        /// 异步获取配置并持有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// 
        public void LoadConfigCacheAsync(string path, Type type, Action<object> callback) 
        {
            _configLoader.LoadConfigCacheAsync(path, type, callback);
        }
        
        /// <summary>
        /// 获取配置并持有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void LoadConfigCache<T>(string path) 
        {
            _configLoader.LoadConfigCache<T>(path);
        }
        
        /// <summary>
        /// 异步获取配置并持有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void LoadConfigCacheAsync<T>(string path, Action<T> callback) 
        {
            _configLoader.LoadConfigCacheAsync<T>(path, callback);
        }

        
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// 
        public object LoadConfigUnCache(string path, Type type) 
        {
            return _configLoader.LoadConfigUnCache(path, type);
        }
        
        /// <summary>
        /// 异步获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// 
        public void LoadConfigUnCacheAsync(string path, Type type, Action<object> callback) 
        {
            _configLoader.LoadConfigCacheAsync(path, type, callback);
        }
        
        
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LoadConfigUnCache<T>(string path)
        {
            return _configLoader.LoadConfigUnCache<T>(path);
        }
        
        /// <summary>
        /// 异步获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void LoadConfigUnCacheAsync<T>(string path, Action<T> callback)
        {
            _configLoader.LoadConfigUnCacheAsync<T>(path, callback);
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return _configLoader.Get<T>();
        }
    }
}
