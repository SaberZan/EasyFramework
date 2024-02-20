using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;

namespace Easy
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigAttribute : Attribute
    {
        public readonly string configName;

        public ConfigAttribute(string id)
        {
            configName = id;
        }
    }

    public abstract class ConfigLoader
    {
        public abstract void BeforeRestart();

        public abstract void Init(InitCompleteCallback complete);

        public abstract object LoadConfigCache(string path, Type type);
        
        public abstract void LoadConfigCacheAsync(string path, Type type, Action<object> callback);
        
        public abstract void LoadConfigCache<T>(string path);

        public abstract void LoadConfigCacheAsync<T>(string path, Action<T> callback);

        public abstract object LoadConfigUnCache(string path, Type type);
        
        public abstract void LoadConfigUnCacheAsync(string path, Type type, Action<object> callback);

        public abstract T LoadConfigUnCache<T>(string path);
        
        public abstract void LoadConfigUnCacheAsync<T>(string path, Action<T> callback);

        public abstract T Get<T>();
    }
}
