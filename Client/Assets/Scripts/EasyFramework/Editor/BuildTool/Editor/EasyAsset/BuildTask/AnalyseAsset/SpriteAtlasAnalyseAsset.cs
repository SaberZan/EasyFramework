using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Easy.EasyAsset
{

    [AnalyseAssetAttribute(".spriteatlas")]
    public class SpriteAtlasAnalyseAsset : IAnalyseAsset
    {
        public List<string> GetDependencies(string assetPath)
        {
            List<string> assetPaths = AssetDatabase.GetDependencies(assetPath, true).ToList();
            for (int i = 0; i < assetPaths.Count; i++)
            {
                string path = assetPaths[i];
                if (path == assetPath)
                {
                    assetPaths.RemoveAt(i);
                    --i;
                    continue;
                }
                if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    assetPaths.RemoveAt(i);
                    --i;
                    string[] inDirFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    assetPaths.AddRange
                    (
                        inDirFiles.Where
                        (
                            inDirfile => 
                            AssetDatabase.GetMainAssetTypeAtPath(inDirfile) == typeof(UnityEngine.Texture2D) 
                            && !assetPaths.Contains(inDirfile) 
                            && inDirfile != assetPath
                        )
                    );
                }
            }
            return assetPaths;
        }
    }



}