using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace Easy.EasyAsset
{
    public enum CopyType 
    {
        CopyVersionAndCatalog,
        JustCopyInStreamAssetBundle,
        CopyAllAssetBundle,
        CopyNone,
    }

    public class GenerateInfo : ScriptableObject
    {
        /// <summary>
        /// 原始文件夹
        /// </summary>
        [Directory]
        public string originPath;
        /// <summary>
        /// 原始文件夹
        /// </summary>
        public string OriginPath => Application.dataPath + "/" + originPath + (originPath.EndsWith("/") ? "AB/" : "/AB/") + build_target.ToString() + "/";
        /// <summary>
        /// 输出文件夹
        /// </summary>
        [Directory]
        public string outputPath;
        /// <summary>
        /// 输出文件夹
        /// </summary>
        public string OutputPath => Application.dataPath + "/" + outputPath + (outputPath.EndsWith("/") ? "AB/" : "/AB/") + build_target.ToString() + "/";
        /// <summary>
        /// ab包操作类型类型
        /// </summary>
        [EnumToggles(ToggleWidth = 200)]
        public BuildAssetBundleOptions buildAssetBuildOptions;
        /// <summary>
        /// ab包针对的目标平台
        /// </summary>
        public BuildTarget build_target;
        /// <summary>
        /// 打到母包下的AB包
        /// </summary>
        public CopyType copyType;
        /// <summary>
        /// 下载更新地址
        /// </summary>
        public List<string> updateurls;

        public List<string> Updateurls
        {
            get 
            {
                List<string> result = new List<string>();
                for(int i = 0; i < updateurls.Count; ++i)
                {
                    result.Add(updateurls[i] + (updateurls[i].EndsWith("/") ? "AB/" : "/AB/") + build_target.ToString() + "/");
                }
                return result;
            }
        }
        /// <summary>
        /// 请求更新地址
        /// </summary>
        public string requestCatalogsUrl;
        /// <summary>
        /// 请求更新地址
        /// </summary>
        public string RequestCatalogsUrl => requestCatalogsUrl + (requestCatalogsUrl.EndsWith("/") ? "AB/" : "/AB/") + build_target.ToString() + "/catalogs.txt";
        /// <summary>
        /// 热更版本号
        /// </summary>
        public long version;
        /// <summary>
        /// 版本号
        /// </summary>
        public long versionCode;
        /// <summary>
        /// 预先生成prebuild
        /// </summary>
        public bool hybridCLRPreBuild;

        public GenerateInfo()
        {
            //build_target = EditorUserBuildSettings.activeBuildTarget;
            // originPath = EasyAssetEditorConst.genPath;
            // outputPath = EasyAssetEditorConst.outPath;
            buildAssetBuildOptions = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode;            
            copyType = CopyType.JustCopyInStreamAssetBundle;
            updateurls = new List<string>(){"http://127.0.0.1:9999/"};
            requestCatalogsUrl = "http://127.0.0.1:9999/";
            version = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            versionCode = 0;
            hybridCLRPreBuild = false;
        }
    }
}