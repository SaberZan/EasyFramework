#if !AA

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;
using System.Linq;

namespace Easy.EasyAsset
{
    public class CheckCircular : ScriptableObject
    {

        [DynamicHelp(nameof(assetResult))]
        [Hide]
        public string assetResult = "";

        [Directory]
        [EditorButton(nameof(CheckCircularAsset), activityType: ButtonActivityType.OnEditMode, ValidateMethodName = nameof(IsCanCheckCircularAsset))]
        public string assetPath;

        public bool IsCanCheckCircularAsset()
        {
            return string.IsNullOrEmpty(assetPath) && File.Exists(assetPath) && File.Exists(EasyAssetEditorConst.EasyAssetConfigPath);
        }

        public void CheckCircularAsset()
        {
            assetResult = "";
            if(string.IsNullOrEmpty(assetPath))
            {
                PackageConfig packageConfig = AssetDatabase.LoadAssetAtPath<PackageConfig>(EasyAssetEditorConst.EasyAssetConfigPath);
                foreach (PackageConfigInfo packageInfo in packageConfig.packageInfos)
                {
                    string packageName = packageInfo.packageName;
                    foreach (GroupConfigInfo groupInfo in packageInfo.groups)
                    {
                        string groupName = groupInfo.groupName;
                        foreach (UnityEngine.Object asset in groupInfo.assets)
                        {
                            string path = AssetDatabase.GetAssetPath(asset);
                            if (Directory.Exists(path))
                            {
                                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                                foreach (string file in files)
                                {
                                    string filePath = file.Replace("\\","/");
                                    CheckCircularAssetInternal(filePath);
                                }
                            }
                            else
                            {
                                string filePath = path.Replace("\\","/");
                                CheckCircularAssetInternal(filePath);
                            }
                        }
                    }
                }
            }
            else
            {
                string filePath = assetPath.Replace("\\","/");
                CheckCircularAssetInternal(filePath);
            }
            if(string.IsNullOrEmpty(assetResult))
            {
                assetResult = "没有循环依赖";
            }
        }

        private void CheckCircularAssetInternal(string assetPath) 
        {
            List<string> files = AnalyseAssetContext.GetDependencies(assetPath).ToList();
            foreach (string file in files)
            {
                List<string> tmps = AnalyseAssetContext.GetDependencies(file).ToList();
                if(tmps.Contains(assetPath))
                {
                    assetResult += assetPath + "\n" + file + "\n\n";
                }
            }
        }


        [DynamicHelp(nameof(bundleResult))]
        [Hide]
        public string bundleResult = "";


        [Directory]
        [EditorButton(nameof(CheckCircularBundle), activityType: ButtonActivityType.OnEditMode, ValidateMethodName = nameof(IsCanCheckCircularBundle))]
        public string manifestPath;

        public bool IsCanCheckCircularBundle()
        {
            return string.IsNullOrEmpty(manifestPath) && File.Exists(manifestPath);
        }

        public void CheckCircularBundle()
        {
            if(string.IsNullOrEmpty(manifestPath))
            {
                bundleResult = "请选文件";
                return;
            }
            bundleResult = "";
            AssetBundle assetBundle = AssetBundle.LoadFromFile(manifestPath);
            if(assetBundle.Contains("assetbundlemanifest"))
            {
                AssetBundleManifest assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest"); 
                string[] bundleNames = assetBundleManifest.GetAllAssetBundles();
                foreach(string bundleName in bundleNames)
                {
                    string[] depends = assetBundleManifest.GetAllDependencies(bundleName);
                    foreach(string depend in depends)
                    {
                        string[] temp = assetBundleManifest.GetAllDependencies(depend);
                        if(temp.Contains(depend))
                        {
                            bundleResult += depend + "\n" + bundleName + "\n\n";
                        }
                    }
                }
            }
            assetBundle.Unload(true);
            if(string.IsNullOrEmpty(bundleResult))
            {
                bundleResult = "没有循环依赖";
            }
        }        
    }
}

#endif