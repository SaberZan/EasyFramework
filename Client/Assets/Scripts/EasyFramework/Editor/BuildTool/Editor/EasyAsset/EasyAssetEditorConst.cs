
using System.Collections.Generic;
using UnityEngine;

namespace Easy.EasyAsset
{
    public static class EasyAssetEditorConst 
    {

        public static string genPath = Application.dataPath + "/AssetBundles/Original/";

        public static string outPath = Application.dataPath + "/AssetBundles/Output/";

        public static string tempPath = Application.persistentDataPath + "/Temp/";

        public static string streamingAssetBundlesPath = Application.streamingAssetsPath + "/AssetBundles/";

        public static string GenerateInfoPath = "Assets/AssetsBundleConfig/GenerateInfo.asset";

        public static string EasyAssetConfigPath = "Assets/AssetsBundleConfig/EasyAssetConfig.asset";

        public static string EasyAssetBuildTaskPipleLinePath = "Assets/AssetsBundleConfig/BuildTaskPipleLine.asset";
        
        public static string HybridCLRBuildCacheDir => "Assets/HybridCLRBuildCache";

        public static string HybridCLRAOT => "Assets/HybridCLRBuildCache/AOT";

        public static string HybridCLRHotUpdate => "Assets/HybridCLRBuildCache/HotUpdate";

        public const string LOAD_CATALOGS = "加载catalogs配置";

        public const string BUILD_AB = "构建AB包";

        public const string LOAD_ASSET_TYPE = "加载资源方式";

        public const string LOAD_ASSET_TYPE_BUNDLE = "ͨ通过Bundle加载";

        public const string LOAD_ASSET_TYPE_ASSET_DATA_BASE = "ͨ通过AssetDataBase加载";

        public const string GEN_INFO = "生成AB包信息配置";

        public const string HOT_UPDATE_TEST_SERVER = "热更测试服务器";

        public const string MUST = "must";

        public const string COMMON = "common";

        public const string HOT_UPDATE_DLLS = "hotupdatedlls";

        public const string AOT_DLLS = "aotdlls";

    }

}