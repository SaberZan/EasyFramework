using System;
using System.Net;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Easy.EasyAsset
{
    public class GenRawAssetBundleTask : IBuildTask
    {
        public string BuildName()
        {
            return "生成原始资源";
        }

        public async Task<BuildResult> Run(GenerateContext context)
        {
            foreach (var kv in context.easyAssetBundleConfigInfos)
            {
                if(kv.Value.easyAssetBundleType == EasyAssetBundleType.RawAssetBundle)
                {
                    byte[] allBytes = new byte[0];
                    foreach (var path in kv.Value.assets)
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        int preLength = allBytes.Length;
                        Array.Resize(ref allBytes, allBytes.Length + (int)fileInfo.Length );
                        fileInfo.OpenRead().Read(allBytes, preLength,  (int)fileInfo.Length);
                    }
                    File.WriteAllBytes(context.generateInfo.OriginPath + kv.Key, allBytes);
                }
            }
            return BuildResult.Success;
        }
    }
}