using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;

namespace Easy.EasyAsset
{
    public class GenAssetBundleAssetTask : IBuildTask
    {
        public string BuildName()
        {
            return "生成打bundle的配置";
        }

        public BuildResult Run(GenerateContext context)
        {
            if (Directory.Exists(EasyAssetEditorConst.tempPath))
                Directory.Delete(EasyAssetEditorConst.tempPath, true);
            Directory.CreateDirectory(EasyAssetEditorConst.tempPath);

            var allAssets = context.allAssets;

            var easyAssetBundleConfigInfos = context.easyAssetBundleConfigInfos;

            //ab包名字命名规则
            foreach (KeyValuePair<string, EasyAssetConfigInfo> keyValuePair in allAssets)
            {
                EasyAssetConfigInfo abAssetConfigInfo = keyValuePair.Value;
                abAssetConfigInfo.beABUsed.Sort();
                string abName = "";
                if(abAssetConfigInfo.isBind)
                {
                    abName = abAssetConfigInfo.abName;
                }
                else if (abAssetConfigInfo.beABUsed.Contains(EasyAssetEditorConst.COMMON))
                {
                    abName = EasyAssetEditorConst.COMMON;
                }
                else if(abAssetConfigInfo.bePackageUsed.Count > 2)
                {
                    abName = string.Concat(abAssetConfigInfo.bePackageUsed);
                }
                else if(abAssetConfigInfo.bePackageUsed.Count == 1 && abAssetConfigInfo.beABUsed.Count >= 5)
                {
                    abName = abAssetConfigInfo.bePackageUsed[0] + EasyAssetEditorConst.COMMON;
                }
                else
                {
                    abName = string.Concat(abAssetConfigInfo.beABUsed);
                }
                //场景文件单独打,另外加_scene 后缀
                if(keyValuePair.Key.EndsWith(".unity"))
                {
                    abName += "_scene";
                }
                abName = MD5Utility.GetMd5Hash(abName);
                abAssetConfigInfo.abName = abName;
                if (!easyAssetBundleConfigInfos.ContainsKey(abName))
                {
                    easyAssetBundleConfigInfos.Add(abName, new EasyAssetBundleConfigInfo() { easyAssetBundleType = abAssetConfigInfo.isRaw ? EasyAssetBundleType.RawAssetBundle : EasyAssetBundleType.UnityAssetBundle });
                }
                easyAssetBundleConfigInfos[abName].assets.Add(abAssetConfigInfo.asset);
                List<string> packages = context.easyAssetBundleConfigInfos[abName].packages;
                packages.AddRange(abAssetConfigInfo.bePackageUsed.Where(package=>!packages.Contains(package)));
                List<string> groups = context.easyAssetBundleConfigInfos[abName].groups;
                groups.AddRange(abAssetConfigInfo.beGroupUsed.Where(group=>!groups.Contains(group)));
            }
            return BuildResult.Success;
        }
        
        private EasyAssetInfo GetAssetInfoByAsset(List<EasyAssetInfo> abAssetInfos, string assetName)
        {
            foreach (var abAssetInfo in abAssetInfos)
            {
                if(abAssetInfo.asset == assetName)
                {
                    return abAssetInfo;
                }
            }
            return null;
        }
    }
}