using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using System.Reflection;
using System.Threading.Tasks;

namespace Easy.EasyAsset
{

    public class AnalyseAssetsTask : IBuildTask
    {
        private Dictionary<string, EasyAssetConfigInfo> allAssets; 

        private PackageConfig _packageConfig;

        private KeyToAsset _keyToAsset;

        public string BuildName()
        {
            return "分析资源";
        }

        public async Task<BuildResult> Run(GenerateContext context)
        {
            _packageConfig = context.packageConfig;
            if (_packageConfig == null || _packageConfig.packageInfos.Count == 0)
            {
                EditorUtility.DisplayProgressBar("打包AB包", "无AB包", 0);
                EditorUtility.ClearProgressBar();
                return BuildResult.Fail;
            }

            allAssets = context.allAssets;
            //统计资源
            foreach (PackageConfigInfo packageInfo in _packageConfig.packageInfos)
            {
                string packageName = packageInfo.packageName;
                foreach (GroupConfigInfo abGroupInfo in packageInfo.groups)
                {
                    string groupName = abGroupInfo.groupName;
                    foreach (UnityEngine.Object asset in abGroupInfo.assets)
                    {
                        if(asset == null)
                        {
                            continue;
                        }

                        string path = AssetDatabase.GetAssetPath(asset);
                        if (Directory.Exists(path))
                        {
                            string directory = new DirectoryInfo(path).Name;
                            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                            foreach (string file in files)
                            {
                                string assetPath = file;
                                StartGenAssetConfigInfo(assetPath, packageName, groupName, directory, abGroupInfo.packType, abGroupInfo.isBind, abGroupInfo.isRaw);
                            }
                        }
                        else
                        {
                            string assetPath = path;
                            StartGenAssetConfigInfo(assetPath,packageName, groupName, "", abGroupInfo.packType, abGroupInfo.isBind, abGroupInfo.isRaw);
                        }
                    }
                } 
            }
            allAssets = null;
            return BuildResult.Success;
        }


                
        private void StartGenAssetConfigInfo(string assetPath, string packageName, string groupName, string topDirectory, PackType packType, bool isBind, bool isRaw)
        {
            if (Array.Exists<string>(_packageConfig.IgnoreAssets,ex=> assetPath.EndsWith(ex)))
            {
                return;
            }
            assetPath = assetPath.Replace("\\", "/");
            string abName = "";
            if(packType == PackType.PackTogger)
            {
                abName = packageName + groupName;
            }
            else if(packType == PackType.PackTopDirectroy)
            {
                abName = packageName + groupName + topDirectory;
            }
            else if(packType == PackType.PackSingle)
            {
                abName = packageName + groupName + Path.GetFileNameWithoutExtension(assetPath).ToLower();
            }

            GenAssetConfigInfo(assetPath, abName, packageName, packageName + groupName, isBind, isRaw);

        }

        private EasyAssetConfigInfo GetAssetConfigInfo(string assetPath)
        {
            if(!allAssets.ContainsKey(assetPath))
            {
                EasyAssetConfigInfo abAssetConfigInfo = new EasyAssetConfigInfo() 
                {
                    keys = _keyToAsset.GetKeys(assetPath),
                    asset = assetPath, 
                    type = GetAssetType(assetPath),
                    changeTag = File.GetLastWriteTimeUtc(assetPath).Ticks
                };
                allAssets.Add(assetPath, abAssetConfigInfo);
            }
            return allAssets[assetPath];
        }

        /// <summary>
        /// 信息分析生成
        /// </summary>
        private void GenAssetConfigInfo(string assetPath, string abName, string packageName, string groupName, bool isBind, bool isRaw)
        {
            EasyAssetConfigInfo abAssetConfigInfo = GetAssetConfigInfo(assetPath);
            abAssetConfigInfo.AddBeUsedAB(abName);
            abAssetConfigInfo.AddBeUsedGroup(groupName);
            abAssetConfigInfo.AddBeUsedPackage(packageName);
            abAssetConfigInfo.isBind = abAssetConfigInfo.isBind || isBind;
            abAssetConfigInfo.isRaw = isRaw;
            abAssetConfigInfo.isActive = true;
            if(isBind)
            {
                if(!string.IsNullOrEmpty(abAssetConfigInfo.abName))
                {
                    throw new Exception("BindResource can not in other group");
                }
                abAssetConfigInfo.abName = abName;
            }
            
            List<string> files = AnalyseAssetContext.GetDependencies(assetPath);
            if(isRaw && files.Count > 0)
            {
                throw new Exception("RawResource can not has dependencies");
            }
            files.RemoveAll(filePath => Array.Exists<string>(_packageConfig.IgnoreAssets, ex => filePath.EndsWith(ex)) || filePath == assetPath);
            foreach (string file in files)
            {
                EasyAssetConfigInfo childAssetConfigInfo = GetAssetConfigInfo(file);
                childAssetConfigInfo.AddBeUsedAB(abName);
                childAssetConfigInfo.AddBeUsedGroup(groupName);
                childAssetConfigInfo.AddBeUsedPackage(packageName);
                childAssetConfigInfo.isBind = abAssetConfigInfo.isBind || isBind;
                if(isBind)
                {
                    if(!string.IsNullOrEmpty(childAssetConfigInfo.abName))
                    {
                        throw new Exception("BindResource can not in other group");
                    }
                    childAssetConfigInfo.abName = abName;
                }
                abAssetConfigInfo.AddChildAsset(childAssetConfigInfo);
            }
        }

        private Type GetAssetType(string assetPath)
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            while(type != null)
            {
                if(!type.Assembly.GetName().Name.Contains("UnityEditor"))
                {
                    break;
                }
                type = type.BaseType;
            }
            if(type == null)
            {
                throw new Exception("类型不能为空");
            }
            return type;
        }
    }
}