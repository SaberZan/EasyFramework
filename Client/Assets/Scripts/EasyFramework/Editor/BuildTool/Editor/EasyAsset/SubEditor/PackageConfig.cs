using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Easy.EasyAsset
{
    public enum PackType
    {
        PackTogger,
        PackTopDirectroy,
        PackSingle,
    }

    [Serializable]
    public class PackageConfig : ScriptableObject
    {
        /// <summary>
        /// 强制在StreamAssets下的字段
        /// </summary>
        [ReorderableList(ListStyle.Round, elementLabel: "需要进首包", Foldable = true)]
        public string[] customInStreamAssets = new string[] { EasyAssetEditorConst.MUST, EasyAssetEditorConst.COMMON };

        /// <summary>
        /// 必须下载的字段
        /// </summary>
        [ReorderableList(ListStyle.Round, elementLabel: "必须下载的字段", Foldable = true)]
        public string[] mustDowmload = new string[] { EasyAssetEditorConst.MUST, EasyAssetEditorConst.COMMON };

        /// <summary>
        /// 忽略的资源
        /// </summary>
        [ReorderableList(ListStyle.Round, elementLabel: "忽略的资源", Foldable = true)]
        public string[] IgnoreAssets = new string[] { ".cs", ".meta" };
        
        [ReorderableList(ListStyle.Round, elementLabel: "配置", Foldable = true)]
        public List<PackageConfigInfo> packageInfos = new List<PackageConfigInfo>();

        public Location GetLocation(List<string> abGroups)
        {
            foreach (var item in customInStreamAssets)
            {
                if (abGroups.Contains(item))
                {
                    return Location.InStreamAsset;
                }
            }

            foreach (PackageConfigInfo packageInfo in packageInfos)
            {
                foreach (var group in packageInfo.groups)
                {
                    if (abGroups.Contains(packageInfo.packageName + group.groupName) && group.location == Location.InStreamAsset)
                    {
                        return Location.InStreamAsset;
                    }
                }
            }

            return Location.InServer;
        }

        public bool IsEncrypt(List<string> abGroups)
        {
            foreach (PackageConfigInfo packageInfo in packageInfos)
            {
                foreach (var group in packageInfo.groups)
                {
                    if (abGroups.Contains(group.groupName.Replace(packageInfo.packageName,"")) && group.isEncrypt)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public DownloadPriority GetDownloadPriority(List<string> groups)
        {
            List<DownloadPriority> downloadPriorityTypes = new List<DownloadPriority>();

            foreach (var item in mustDowmload)
            {
                if (groups.Contains(item))
                {
                    if (!downloadPriorityTypes.Contains(DownloadPriority.Must))
                    {
                        downloadPriorityTypes.Add(DownloadPriority.Must);
                    }
                }
            }

            foreach (PackageConfigInfo packageInfo in packageInfos)
            {
                foreach (var group in packageInfo.groups)
                {
                    if(!downloadPriorityTypes.Contains(group.downloadPriorityType))
                    {
                        downloadPriorityTypes.Add(group.downloadPriorityType);
                    }
                }
            }
            downloadPriorityTypes.Sort((a,b)=>
            {
                return a - b;
            });
            return downloadPriorityTypes[0];
        }


        [InitializeOnEnterPlayMode]
        static void ListenEnterPlayMode()
        {
            PackageConfig abConfig = AssetDatabase.LoadAssetAtPath<PackageConfig>(EasyAssetEditorConst.EasyAssetConfigPath);
            if (abConfig != null)
            {
                Catalogs abCatalogs = PackageConfig.ABPackageConfigToCatalogs(abConfig);
                string abJsonStr = JsonUtility.ToJson(abCatalogs);
                byte[] buffer = Encoding.UTF8.GetBytes(abJsonStr);
                XOREncryption.EncryptData(buffer, 0 , -1, XOREncryption.DEFAULT_ENCRYPT_KEY, buffer.Length);
                File.WriteAllBytes(Application.persistentDataPath + "/editorCatalogs.txt", buffer);
            }
        }
        
        static Catalogs ABPackageConfigToCatalogs(PackageConfig abConfig)
        {
            Catalogs catalogs = new Catalogs();
            foreach (PackageConfigInfo packageInfo in abConfig.packageInfos)
            {
                foreach (GroupConfigInfo groupInfo in packageInfo.groups)
                {
                    EasyAssetBundleInfo abInfo = new EasyAssetBundleInfo();
                    abInfo.abName = groupInfo.groupName;
                    abInfo.md5 = packageInfo.packageName;
                    abInfo.easyAssetBundleType = groupInfo.isRaw ? EasyAssetBundleType.RawAssetBundle : EasyAssetBundleType.UnityAssetBundle;
                    foreach (UnityEngine.Object asset in groupInfo.assets)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(asset);
                        if (Directory.Exists(assetPath))
                        {
                            string[] files = Directory.GetFiles(assetPath, "*.*", SearchOption.AllDirectories);
                            foreach (string file in files)
                            {
                                string filePath = file.Replace("\\", "/");
                                if (filePath.EndsWith(".meta") || filePath.EndsWith(".cs"))
                                {
                                    continue;
                                }

                                EasyAssetInfo abAssetInfo = new EasyAssetInfo();
                                abAssetInfo.asset = filePath;
                                abAssetInfo.abIndex = catalogs.allEasyAssetBundleInfos.Count;
                                abAssetInfo.needABIndexes = new List<int>();
                                abAssetInfo.changeTag = 0;
                                abAssetInfo.isRaw = groupInfo.isRaw;
                                if (!groupInfo.isRaw)
                                {
                                    var temp = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
                                    string assemblyQualifiedName = temp.GetType().AssemblyQualifiedName;
                                    int typeIndex = catalogs.assemblyQualifiedNames.IndexOf(assemblyQualifiedName);
                                    if (typeIndex == -1)
                                    {
                                        typeIndex = catalogs.assemblyQualifiedNames.Count;
                                        catalogs.assemblyQualifiedNames.Add(assemblyQualifiedName);
                                    }

                                    abAssetInfo.typeIndex = typeIndex;
                                }

                                catalogs.allActiveEasyAssetInfos.Add(abAssetInfo);
                            }
                        }
                        else
                        {
                            string file = assetPath;
                            string filePath = file.Replace("\\", "/");
                            if (file.EndsWith(".meta") || file.EndsWith(".cs"))
                            {
                                continue;
                            }
                            EasyAssetInfo abAssetInfo = new EasyAssetInfo();
                            abAssetInfo.asset = filePath;
                            abAssetInfo.abIndex = catalogs.allEasyAssetBundleInfos.Count;
                            abAssetInfo.needABIndexes = new List<int>();
                            abAssetInfo.changeTag = 0;
                            abAssetInfo.isRaw = groupInfo.isRaw;
                            if (!groupInfo.isRaw)
                            {
                                var temp = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
                                string assemblyQualifiedName = temp.GetType().AssemblyQualifiedName;
                                int typeIndex = catalogs.assemblyQualifiedNames.IndexOf(assemblyQualifiedName);
                                if (typeIndex == -1)
                                {
                                    typeIndex = catalogs.assemblyQualifiedNames.Count;
                                    catalogs.assemblyQualifiedNames.Add(assemblyQualifiedName);
                                }
                                abAssetInfo.typeIndex = typeIndex;
                            }
                            catalogs.allActiveEasyAssetInfos.Add(abAssetInfo);
                        }
                    }
                    catalogs.allEasyAssetBundleInfos.Add(abInfo);
                }                
            }
            return catalogs;
        }
    }
    
}
