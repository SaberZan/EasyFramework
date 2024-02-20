using System;
using System.IO;
using UnityEngine;

namespace Easy.EasyAsset
{
    public class UnityAssetBundle : EasyAssetBundle
    {
        public AssetBundle assetBundle;
        public UnityAssetBundle(EasyAssetsLoader loader, int index) : base(loader, index)
        {
            EasyAssetBundleInfo easyAssetBundleInfo = loader.catalogs.GetEasyAssetBundleInfoByIndex(index);
#if UNITY_ANDROID
            if (loader.playAssetPackCtrl.IsPlayerAssetContains(easyAssetBundleInfo.md5, out _))
            {
                assetBundle = loader.playAssetPackCtrl.LoadFromPlayAssetDelivery(easyAssetBundleInfo.md5, easyAssetBundleInfo.isEncrypt);
            }
            else
            {
                assetBundle = LoadFromPersistentDataPath(easyAssetBundleInfo.md5, easyAssetBundleInfo.isEncrypt);
            }  
#else
            assetBundle = LoadFromPersistentDataPath(easyAssetBundleInfo.md5, easyAssetBundleInfo.isEncrypt);
#endif
        }

        public T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            return assetBundle.LoadAsset<T>(path);
        }

        public UnityEngine.Object LoadAsset(string path, Type type)
        {
            return assetBundle.LoadAsset(path);
        }

        public override void Unload(bool value)
        {
            assetBundle?.Unload(value);
        }

        /// <summary>
        /// 加载ab包通过可读写路径
        /// </summary>
        private static AssetBundle LoadFromPersistentDataPath(string md5, bool isEncrypt)
        {
            string fullPath = Const.localAssetBundleFolder + md5;
            if (!File.Exists(fullPath))
            {
                EasyLogger.LogError("EasyFrameWork", $"***** Need Load File Lost {fullPath}*****");
                return null;
            }
            AssetBundle assetBundle = null;
            if(isEncrypt)
            {
                // byte[] bytes = File.ReadAllBytes(fullPath);
                // XOREncryption.DecryptData(bytes,0,-1, XOREncryption.DEFAULT_ENCRYPT_KEY, bytes.Length);
                // assetBundle = AssetBundle.LoadFromMemory(bytes);
                XOREncryptFileStream abFileStream = new XOREncryptFileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 4, false);
                assetBundle = AssetBundle.LoadFromStream(abFileStream);
            }
            else
            {
                assetBundle = AssetBundle.LoadFromFile(fullPath);
            }
            return assetBundle;
        }
    }
}