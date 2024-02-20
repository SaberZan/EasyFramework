#if UNITY_ANDROID

using Easy;
using Easy.EasyAsset;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayAssetPackCtrl
{
    PlayAssetPackConfig config;

    public PlayAssetPackCtrl()
    {
        PlayAssetPackConfig assetPackConfig = JsonUtility.FromJson<PlayAssetPackConfig>(Const.originalPlayAssetPackPath);
    }

    public bool IsAssetBundleDownloaded(string md5)
    {
        if(IsPlayerAssetContains(md5, out PlayAssetPackConfigInfo configInfo))
        {
            return Google.Play.AssetDelivery.PlayAssetDelivery.IsDownloaded(configInfo.assetPackName);
        }
        return false;
    }

    public bool IsPlayerAssetContains(string md5, out PlayAssetPackConfigInfo configInfo)
    {
        configInfo = null;
        for (int i = 0; i < config.configInfos.Count; i++)
        {
            PlayAssetPackConfigInfo playAssetPackConfigInfo = config.configInfos[i];
            if(playAssetPackConfigInfo.assetBundleNames.Contains(md5))
            {
                configInfo = playAssetPackConfigInfo;
                return true;
            }
        }
        return false;
    }

    public Stream LoadRawFromPlayAssetDelivery(string md5, bool isEncrypt, out long offset)
    {
        offset = 0;
        if (IsPlayerAssetContains(md5, out PlayAssetPackConfigInfo configInfo))
        {
            var request = Google.Play.AssetDelivery.PlayAssetDelivery.RetrieveAssetPackAsync(configInfo.assetPackName, true);
            while (!request.IsDone)
            {

            }
            var assetLocation = request.GetAssetLocation(md5);
            string fullPath = assetLocation.Path;
            FileStream fileStream = null;
            if (isEncrypt)
            {
                fileStream = new XOREncryptFileStream(fullPath, FileMode.Open);
            }
            else
            {
                fileStream = new FileStream(fullPath, FileMode.Open);
            }
            offset = (long)assetLocation.Offset;
            fileStream.Seek(offset, SeekOrigin.Begin);
            return fileStream;
        }
        return null;
    }

    public AssetBundle LoadFromPlayAssetDelivery(string md5, bool isEncrypt)
    {
        if (IsPlayerAssetContains(md5, out PlayAssetPackConfigInfo configInfo))
        {
            AssetBundle assetBundle = null;
            var request = Google.Play.AssetDelivery.PlayAssetDelivery.RetrieveAssetPackAsync(configInfo.assetPackName);
            while (!request.IsDone)
            {

            }
            var assetLocation = request.GetAssetLocation(md5);
            string fullPath = assetLocation.Path;
            if (isEncrypt)
            {
                // byte[] bytes = File.ReadAllBytes(fullPath);
                // XOREncryption.DecryptData(bytes,0,-1, XOREncryption.DEFAULT_ENCRYPT_KEY, bytes.Length);
                // assetBundle = AssetBundle.LoadFromMemory(bytes);
                // ABFileStream abFileStream = new ABFileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 4, false);
                // abFileStream.Seek((long) assetLocation.Offset, SeekOrigin.Begin);
                // assetBundle = AssetBundle.LoadFromStream(abFileStream);

                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                XOREncryptStream abStream = new XOREncryptStream(fileStream, (int)assetLocation.Offset, (long)assetLocation.Size);
                assetBundle = AssetBundle.LoadFromStream(abStream);
            }
            else
            {
                assetBundle = AssetBundle.LoadFromFile(fullPath, 0, assetLocation.Size);
            }
            return assetBundle;
        }
        return null;
    }
}

#endif