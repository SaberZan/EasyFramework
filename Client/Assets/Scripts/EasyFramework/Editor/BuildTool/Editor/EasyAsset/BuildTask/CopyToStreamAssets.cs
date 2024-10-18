using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

namespace Easy.EasyAsset
{
    public class CopyToStreamAssets : IBuildTask
    {
        public string BuildName()
        {
            return "拷贝到StreamAssets";
        }

        public async Task<BuildResult> Run(GenerateContext context)
        {
            if(context.generateInfo.copyType != CopyType.CopyNone)
            {
                if (Directory.Exists(EasyAssetEditorConst.streamingAssetBundlesPath))
                    Directory.Delete(EasyAssetEditorConst.streamingAssetBundlesPath, true);
                Directory.CreateDirectory(EasyAssetEditorConst.streamingAssetBundlesPath);
            }

            foreach (var abInfo in context.catalogs.allEasyAssetBundleInfos)
            {
                List<string> packages = abInfo.packages;
                packages.Sort();
                string originPath = Path.Combine(context.generateInfo.OutputPath, 
                    abInfo.location.ToString(), 
                    abInfo.abDownloadPriority.ToString(), 
                    string.Join("_", packages), 
                    abInfo.md5);
                string targetPath = EasyAssetEditorConst.streamingAssetBundlesPath + abInfo.md5;
                if (context.generateInfo.copyType == CopyType.CopyAllAssetBundle)
                {
                    File.Copy(originPath, targetPath, true);
                }
                else if(context.generateInfo.copyType == CopyType.JustCopyInStreamAssetBundle)
                {
                    if (abInfo.location == Location.InStreamAsset)
                    {
                        File.Copy(originPath, targetPath, true);
                    }
                }
            }
            
            if (context.generateInfo.copyType != CopyType.CopyNone)
            {
                string originPath = context.generateInfo.OutputPath + "catalogs.txt";
                string targetPath = EasyAssetEditorConst.streamingAssetBundlesPath + "catalogs.txt";
                File.Copy(originPath, targetPath, true);
            }
            
            AssetDatabase.Refresh();
            
            return BuildResult.Success;
        }
    }
}