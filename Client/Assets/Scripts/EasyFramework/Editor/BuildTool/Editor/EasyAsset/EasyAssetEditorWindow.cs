#if !AA

using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine.UIElements;
using Toolbox.Editor.Wizards;
using Toolbox.Editor;

namespace Easy.EasyAsset
{
    public class EasyAssetEditorWindow : ToolboxWizard
    {
        [SpaceArea]
        [Label("资源构建资源信息")]
        [InLineEditorAttribute(true, false, HideScript = true, DrawPropertyField = false)]
        public PackageConfig packageConfig;

        [SpaceArea]
        [Label("资源构建目标信息")]
        [InLineEditorAttribute(true, false, HideScript = true, DrawPropertyField = false)]
        public GenerateInfo generateInfo;

        [SpaceArea]
        [Label("资源构建任务")]
        [InLineEditorAttribute(true, false, HideScript =  true, DrawPropertyField = false)]
        public BuildTaskPipeLine buildTaskPipeLine;

        [SpaceArea]
        [Label("热更服务器")]
        [InLineEditorAttribute(true, false, HideScript =  true, DrawPropertyField = false)]
        public UpdateServer updateServer;

        [SpaceArea]
        [Label("资源循环依赖查询")]
        [InLineEditorAttribute(true, false, HideScript =  true, DrawPropertyField = false)]
        public CheckCircular checkCircular;

        [SpaceArea]
        [Label("资源加载类型")]
        [InLineEditorAttribute(true, false, HideScript =  true, DrawPropertyField = false)]
        public LoadType loadType;

        [MenuItem("Easy/EasyAssetEditor", false)]
        static void OpenWindow()
        {
            var window = ToolboxWizard.DisplayWizard<EasyAssetEditorWindow>("EasyAssetEditor");
            var packageConfig = AssetDatabase.LoadAssetAtPath<PackageConfig>(EasyAssetEditorConst.EasyAssetConfigPath);
            if (packageConfig == null)
            {
                packageConfig = ScriptableObject.CreateInstance<PackageConfig>();
                AssetDatabase.CreateAsset(packageConfig, EasyAssetEditorConst.EasyAssetConfigPath);
            }
            window.packageConfig = packageConfig;

            var generateInfo = AssetDatabase.LoadAssetAtPath<GenerateInfo>(EasyAssetEditorConst.GenerateInfoPath);
            if (generateInfo == null)
            {
                generateInfo = ScriptableObject.CreateInstance<GenerateInfo>();
                AssetDatabase.CreateAsset(generateInfo, EasyAssetEditorConst.GenerateInfoPath);
            }
            generateInfo.version = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            window.generateInfo = generateInfo;

            var buildTaskPipeLine = AssetDatabase.LoadAssetAtPath<BuildTaskPipeLine>(EasyAssetEditorConst.EasyAssetBuildTaskPipleLinePath);
            if (buildTaskPipeLine == null)
            {
                buildTaskPipeLine = ScriptableObject.CreateInstance<BuildTaskPipeLine>();
                AssetDatabase.CreateAsset(buildTaskPipeLine, EasyAssetEditorConst.EasyAssetBuildTaskPipleLinePath);
            }
            window.buildTaskPipeLine = buildTaskPipeLine;
            
            window.updateServer = ScriptableObject.CreateInstance<UpdateServer>();
            window.checkCircular = ScriptableObject.CreateInstance<CheckCircular>();
            window.loadType = ScriptableObject.CreateInstance<LoadType>();
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        protected override bool HandleCreateButton()
        {
            return false;
        }
    }
}

#endif