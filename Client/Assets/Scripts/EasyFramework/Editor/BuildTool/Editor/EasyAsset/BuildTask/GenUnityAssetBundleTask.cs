using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Easy.EasyAsset
{
    public class GenUnityAssetBundleTask : IBuildTask
    {
        public string BuildName()
        {
            return "生成Bundle";
        }

        public BuildResult Run(GenerateContext context)
        {
            if (Directory.Exists(context.generateInfo.OriginPath))
                Directory.Delete(context.generateInfo.OriginPath, true);
            Directory.CreateDirectory(context.generateInfo.OriginPath);
            
            //计算要构建的ab包
            List<UnityEditor.AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
            foreach (var kv in context.easyAssetBundleConfigInfos)
            {
                if (kv.Value.easyAssetBundleType == EasyAssetBundleType.UnityAssetBundle)
                {
                    assetBundleBuilds.Add(new AssetBundleBuild
                    {
                        assetBundleName = kv.Key,
                        assetNames = kv.Value.assets.ToArray()
                    });
                }
            }

            // 生成AB包
            BuildPipeline.BuildAssetBundles(context.generateInfo.OriginPath, assetBundleBuilds.ToArray(), context.generateInfo.buildAssetBuildOptions, context.generateInfo.build_target);
            var files = Directory.GetFiles(EasyAssetEditorConst.tempPath);
            foreach (var file in files)
            {
                File.Move(file, context.generateInfo.OriginPath + Path.GetFileName(file));
            }
            return BuildResult.Success;
        }
    }
}