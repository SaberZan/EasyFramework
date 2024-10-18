using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace Easy.EasyAsset
{
    public class HyBridCLRTask : IBuildTask
    {
        public string BuildName()
        {
            return "c#代码热更新";
        }

        public async Task<BuildResult> Run(GenerateContext context)
        {
            if (Directory.Exists(EasyAssetEditorConst.HybridCLRAOT))
                Directory.Delete(EasyAssetEditorConst.HybridCLRAOT, true);
            Directory.CreateDirectory(EasyAssetEditorConst.HybridCLRAOT);
            if (Directory.Exists(EasyAssetEditorConst.HybridCLRHotUpdate))
                Directory.Delete(EasyAssetEditorConst.HybridCLRHotUpdate, true);
            Directory.CreateDirectory(EasyAssetEditorConst.HybridCLRHotUpdate);

            AssetDatabase.Refresh();
                
            //hybrid----

            if (context.generateInfo.hybridCLRPreBuild)
            {
                PrebuildCommand.GenerateAll();
            }
            CompileDllCommand.CompileDll(context.generateInfo.build_target);

            //AOT-------
            string aotDllDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(context.generateInfo.build_target);
            foreach (var dll in SettingsUtil.HybridCLRSettings.patchAOTAssemblies)
            {
                string dllPath = $"{aotDllDir}/{dll}.dll";
                if (!File.Exists(dllPath))
                {
                    Debug.LogError($"ab中添加AOT补充元数据dll:{dllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }
                string dllBytesPath = $"{EasyAssetEditorConst.HybridCLRAOT}/{dll}.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                Debug.Log($"[BuildAssetBundles] copy AOT dll {dllPath} -> {dllBytesPath}");
            }
            //hotUpdate---------
            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(context.generateInfo.build_target);
            List<string> allHotUpdateDllNames = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
            List<string> allHotUpdateDllFiles = SettingsUtil.HotUpdateAssemblyFilesExcludePreserved;
            foreach (var dll in allHotUpdateDllNames)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}.dll";
                string dllBytesPath = $"{EasyAssetEditorConst.HybridCLRHotUpdate}/{dll}.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                Debug.Log($"[BuildAssetBundles] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }

            AssetDatabase.Refresh();
            
            for (int i = 0; i < context.packageConfig.packageInfos.Count; ++i)
            {
                if(context.packageConfig.packageInfos[i].packageName.Contains("HybridCLR"))
                {
                    context.packageConfig.packageInfos.RemoveAt(i);
                    --i;
                }
            }


            PackageConfigInfo packageConfigInfo = new PackageConfigInfo();
            packageConfigInfo.packageName = "HybridCLR";
            packageConfigInfo.groups = new List<GroupConfigInfo>();
            
            //AOT------------
            {
                GroupConfigInfo groupConfigInfo = new GroupConfigInfo();
                groupConfigInfo.groupName = EasyAssetEditorConst.AOT_DLLS;
                groupConfigInfo.isEncrypt = true;
                groupConfigInfo.downloadPriorityType = DownloadPriority.Must;
                groupConfigInfo.isRaw = false;
                groupConfigInfo.packType = PackType.PackTogger;
                groupConfigInfo.assets = new List<UnityEngine.Object>();
                groupConfigInfo.assets.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(EasyAssetEditorConst.HybridCLRAOT));
                packageConfigInfo.groups.Add(groupConfigInfo);
            }

            //hotupdate-------
            {
                GroupConfigInfo groupConfigInfo = new GroupConfigInfo();
                groupConfigInfo.groupName = EasyAssetEditorConst.HOT_UPDATE_DLLS;
                groupConfigInfo.isEncrypt = true;
                groupConfigInfo.downloadPriorityType = DownloadPriority.Must;
                groupConfigInfo.isRaw = false;
                groupConfigInfo.packType = PackType.PackTogger;
                groupConfigInfo.assets = new List<UnityEngine.Object>();
                groupConfigInfo.assets.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(EasyAssetEditorConst.HybridCLRHotUpdate));
                packageConfigInfo.groups.Add(groupConfigInfo);
            }

            context.packageConfig.packageInfos.Insert(0, packageConfigInfo);
            
            return BuildResult.Success;
        }
    }
}