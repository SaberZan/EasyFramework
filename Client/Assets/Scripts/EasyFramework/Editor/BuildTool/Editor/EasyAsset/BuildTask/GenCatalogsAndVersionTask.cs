using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace Easy.EasyAsset
{
    public class GenCatalogsAndVersionTask : IBuildTask
    {
        public string BuildName()
        {
            return "生成资源总表";
        }

        public BuildResult Run(GenerateContext context)
        {
            Version version = new Version();
            Catalogs catalogs = new Catalogs();
            
            Dictionary<string, EasyAssetConfigInfo> allAssets = context.allAssets;
            List<string> abNames = context.easyAssetBundleConfigInfos.Keys.ToList();

            // 所有资源
            foreach (KeyValuePair<string, EasyAssetConfigInfo> kv in allAssets)
            {
                EasyAssetConfigInfo easyAssetConfigInfo = kv.Value;

                EasyAssetInfo easyAssetInfo = new EasyAssetInfo();
                easyAssetInfo.asset = easyAssetConfigInfo.asset;
                easyAssetInfo.abIndex  = abNames.IndexOf(easyAssetConfigInfo.abName);
                easyAssetInfo.needABIndexes = new List<int>() { easyAssetInfo.abIndex };
                easyAssetInfo.isRaw = easyAssetConfigInfo.isRaw;
                if (easyAssetConfigInfo.isRaw)
                {
                    long offset = 0;
                    List<string> allFiles = context.easyAssetBundleConfigInfos[easyAssetConfigInfo.abName].assets;
                    FileInfo fileInfo = null;
                    for (int i = 0; i < allFiles.Count; ++i)
                    {
                        fileInfo = new FileInfo(allFiles[i]);
                        if (!fileInfo.Exists)
                        {
                            throw new Exception(allFiles[i] + " file is not exist");
                        }
                        if (allFiles[i].Equals(easyAssetInfo.asset))
                        {
                            break;
                        }
                        offset += fileInfo.Length;
                    }
                    easyAssetInfo.offset = offset;
                    easyAssetInfo.size = fileInfo.Length;
                }
                else
                {
                    List<string> dependABs = easyAssetConfigInfo.GetDependABs();
                    for (int i = 0; i < dependABs.Count; ++i)
                    {
                        int index = abNames.IndexOf(dependABs[i]);
                        if (!easyAssetInfo.needABIndexes.Contains(index))
                        {
                            easyAssetInfo.needABIndexes.Add(index);
                        }
                    }
                    string assemblyQualifiedName = easyAssetConfigInfo.type.AssemblyQualifiedName;
                    int typeIndex = catalogs.assemblyQualifiedNames.IndexOf(assemblyQualifiedName);
                    if (typeIndex == -1)
                    {
                        typeIndex = catalogs.types.Count;
                        catalogs.assemblyQualifiedNames.Add(assemblyQualifiedName);
                    }
                    easyAssetInfo.typeIndex = typeIndex;
                }
                easyAssetInfo.changeTag = easyAssetConfigInfo.changeTag;
                if(easyAssetConfigInfo.isActive)
                {
                    catalogs.allActiveEasyAssetInfos.Add(easyAssetInfo);
                }
                else
                {
                   catalogs.allPassiveEasyAssetInfos.Add(easyAssetInfo); 
                }
            }

            // 所有AB包
            foreach (var kv in  context.easyAssetBundleConfigInfos)
            {
                string abName = kv.Key;

                uint crc = 0;
                if (kv.Value.easyAssetBundleType == EasyAssetBundleType.UnityAssetBundle)
                {
                    BuildPipeline.GetCRCForAssetBundle(context.generateInfo.OriginPath + abName, out crc);
                }
                EasyAssetBundleInfo abInfo = new EasyAssetBundleInfo();
                abInfo.abName = abName;
                abInfo.md5 = abName;
                abInfo.crc = crc;
                abInfo.easyAssetBundleType = kv.Value.easyAssetBundleType;
                abInfo.packages = kv.Value.packages;
                List<string> groups = kv.Value.groups;
                string abOriginPath = context.generateInfo.OriginPath + abInfo.md5;
                int pos = abOriginPath.LastIndexOf("/Assets/", StringComparison.Ordinal);
                abOriginPath = abOriginPath.Substring(pos + 1);
                FileInfo fileInfo = new FileInfo(abOriginPath);
                abInfo.size = fileInfo.Length;
                abInfo.location = context.packageConfig.GetLocation(groups);
                abInfo.isEncrypt = context.packageConfig.IsEncrypt(groups);
                abInfo.abDownloadPriority = context.packageConfig.GetDownloadPriority(groups);
                catalogs.allEasyAssetBundleInfos.Add(abInfo);
            }
            
            catalogs.updateUrls = context.generateInfo.Updateurls;
            catalogs.version = context.generateInfo.version;
            catalogs.versionCode = context.generateInfo.versionCode;
            string catalogsJsonStr = JsonConvert.SerializeObject(catalogs);
            byte[] catalogsBuffer = Encoding.UTF8.GetBytes(catalogsJsonStr);
            File.WriteAllBytes(context.generateInfo.OriginPath + "catalogs.txt", catalogsBuffer);
            context.catalogs = catalogs;

            version.version = context.generateInfo.version;
            version.versionCode = context.generateInfo.versionCode;
            version.requestCatalogsUrl = context.generateInfo.RequestCatalogsUrl;
            
            string versionJsonStr = JsonConvert.SerializeObject(version);
            byte[] versionBuffer = Encoding.UTF8.GetBytes(versionJsonStr);
            File.WriteAllBytes(context.generateInfo.OriginPath + "version.txt", versionBuffer);
            context.version = version;

            return BuildResult.Success;
        }
    }
}