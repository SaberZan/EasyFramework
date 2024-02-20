using System;
using UnityEngine;

namespace Easy
{
    public enum ConfigLoaderType
    {
        JSON,
        FLatBuffer
    }
    [Serializable]
    public class ConfigEasyConfig : EasyConfig
    {
        [Label("配置类型")]
        [SerializeField]
        public ConfigLoaderType loaderType = ConfigLoaderType.JSON;

        [Label("配置路径")]
        [Directory]
        [SerializeField]
        public string configPath = "";


        public string GetConfigPath()
        {
            return "Assets/" + configPath;
        }

    }
}