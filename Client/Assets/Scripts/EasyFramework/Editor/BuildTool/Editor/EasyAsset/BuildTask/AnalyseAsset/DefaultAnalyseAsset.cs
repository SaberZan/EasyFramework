using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Easy.EasyAsset
{

    [AnalyseAssetAttribute(".*")]
    public class DefaultAnalyseAsset : IAnalyseAsset
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
                    foreach (var inDirfile in inDirFiles)
                    {
                        assetPaths.AddRange
                        (
                            AnalyseAssetContext.GetDependencies(inDirfile).Where
                            (
                                inDirfile =>
                                !assetPaths.Contains(inDirfile)
                                && inDirfile != assetPath
                            )
                        );
                    }
                }
                else
                {
                    assetPaths.AddRange
                    (
                        AnalyseAssetContext.GetDependencies(path).Where
                        (
                            inDirfile =>
                            !assetPaths.Contains(inDirfile)
                            && inDirfile != assetPath
                        )
                    );
                }
            }
            assetPaths.RemoveAll(filePath => filePath.EndsWith(".cs") || filePath.EndsWith(".meta"));
            return assetPaths;
        }
    }
}