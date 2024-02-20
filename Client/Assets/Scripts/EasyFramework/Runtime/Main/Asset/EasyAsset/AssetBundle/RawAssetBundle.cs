using System;
using System.IO;
using UnityEngine;

namespace Easy.EasyAsset
{
    public class RawAssetBundle : EasyAssetBundle
    {
        private Stream stream;

        private long _offset;

        public RawAssetBundle(EasyAssetsLoader loader, int index) : base(loader, index)
        {
            EasyAssetBundleInfo easyAssetBundleInfo = loader.catalogs.GetEasyAssetBundleInfoByIndex(index);
#if UNITY_ANDROID
            if(loader.playAssetPackCtrl.IsPlayerAssetContains(easyAssetBundleInfo.md5, out _))
            {
                stream = loader.playAssetPackCtrl.LoadRawFromPlayAssetDelivery(easyAssetBundleInfo.md5, easyAssetBundleInfo.isEncrypt, out _offset);
            }
            else
            {
                stream = LoadFromPersistentDataPath(easyAssetBundleInfo.md5, easyAssetBundleInfo.isEncrypt);
            }
#else
            stream = LoadFromPersistentDataPath(easyAssetBundleInfo.md5, easyAssetBundleInfo.isEncrypt); 
#endif
        }

        public byte[] LoadAsset(string path)
        {
            var easyAssetInfo = loader.catalogs.GetEasyAssetInfoByAsset(path);
            byte[] bytes = new byte[easyAssetInfo.size];
            stream.Seek(_offset + easyAssetInfo.offset, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int) easyAssetInfo.size);
            return bytes;
        }

        public override void Unload(bool value)
        {
            stream.Dispose();
        }

        private Stream LoadFromPersistentDataPath(string md5, bool isEncrypt)
        {
            _offset = 0;
            if(isEncrypt)
            {
                return new XOREncryptFileStream(Const.localAssetBundleFolder + md5, FileMode.Open);
            }
            return new FileStream(Const.localAssetBundleFolder + md5, FileMode.Open);
        }
    }
}