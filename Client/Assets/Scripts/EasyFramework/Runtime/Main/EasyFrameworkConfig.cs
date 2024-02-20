using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Easy
{
    [Serializable]
    public abstract class EasyConfig
    {

    }

    [Serializable]
    public class EasyFrameworkConfig
    {
        /// <summary>
        /// 框架配置名
        /// </summary>
        public const string SETTINGS_NAME = "EasyFrameworkConfig";

        [SerializeField]
        public List<string> typesAssemblies = new List<string>();

        [SerializeField]
        public List<string> keys = new List<string>();

        [SerializeField]
        public List<string> values = new List<string>();

        [NonSerialized]
        private List<Type> _types;
        public List<Type> Types
        {
            get
            {
                if (_types == null)
                {
                    _types = new List<Type>();
                    Array.ForEach(AppDomain.CurrentDomain.GetAssemblies(), assembly =>
                    {
                        if (typesAssemblies.Contains(assembly.GetName().Name))
                        {
                            _types.AddRange(assembly.GetTypes().ToList());
                        }
                    });
                }
                return _types;
            }
        }

        [NonSerialized]
        public Dictionary<string, EasyConfig> configs = new Dictionary<string, EasyConfig>();
        public T GetEasyConfig<T>() where T : EasyConfig
        {
            string key = typeof(T).Name;
            if(!configs.ContainsKey(key))
            {
                int index = keys.IndexOf(key);
                string value = values[index];
                T t = JsonUtility.FromJson<T>(value);
                configs.Add(key,t);
            }
            return (T)configs[key];
        }
    }
}