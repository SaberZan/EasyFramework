#if UNITY_ANDROID

using Codice.Client.BaseCommands.Fileinfo;
using Easy.EasyAsset;
using Google.Android.AppBundle.Editor;
using Google.Android.AppBundle.Editor.AssetPacks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PAD
{
    public static AssetPackConfig GenAssetPackConfig()
    {
        AssetPackConfig assetPackConfig = new Google.Android.AppBundle.Editor.AssetPackConfig();
        PlayAssetPackConfig playAssetPackConfig = new PlayAssetPackConfig();

        var locationDirPaths = Directory.GetDirectories(GenerateContext.Instance.generateInfo.OutputPath, "*", SearchOption.TopDirectoryOnly);
        foreach ( var locationDirPath in locationDirPaths)
        {
            DirectoryInfo locationDir = new DirectoryInfo(locationDirPath);
            var downloadPriorityDirs = locationDir.GetDirectories();
            foreach ( var downloadPriorityDir in downloadPriorityDirs)
            {
                var packageClassflyDirs = downloadPriorityDir.GetDirectories();
                foreach( var packageClassflyDir in packageClassflyDirs )
                {
                    string assetPackName = locationDir.Name + "__" + downloadPriorityDir.Name + "__" + packageClassflyDir.Name;
                    string relativePath = Path.GetRelativePath(Directory.GetParent(Application.dataPath).FullName, packageClassflyDir.FullName).Replace("\\", "/");
                    AssetPackDeliveryMode mode = GetAssetPackDeliveryMode(locationDir.Name, downloadPriorityDir.Name);
                    assetPackConfig.AddAssetsFolder(assetPackName, relativePath, mode);


                    PlayAssetPackConfigInfo info = new PlayAssetPackConfigInfo();
                    info.assetPackName = assetPackName;
                    long size = 0;
                    var fileInfos = packageClassflyDir.GetFiles();
                    foreach ( var fileInfo in fileInfos)
                    {
                        if(!Path.HasExtension(fileInfo.FullName))
                        {
                            info.assetBundleNames.Add(fileInfo.Name);
                            size += fileInfo.Length;
                        }
                    }
                    info.size = size;
                    playAssetPackConfig.configInfos.Add(info);

                }
            }
        }
        AssetPackConfigSerializer.SaveConfig(assetPackConfig);

        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "AssetBundles/playAssetPackConfig.txt"), JsonUtility.ToJson(playAssetPackConfig));

        return assetPackConfig;
    }

    private static AssetPackDeliveryMode GetAssetPackDeliveryMode(string location, string downloadType)
    {
        if(location == Location.InStreamAsset.ToString())
        {
            return AssetPackDeliveryMode.InstallTime;
        }
        else if(location == Location.InServer.ToString())
        {
            if(downloadType == DownloadPriority.High.ToString())
            {
                return AssetPackDeliveryMode.FastFollow;
            }
        }
        return AssetPackDeliveryMode.OnDemand;
    }
}

#endif