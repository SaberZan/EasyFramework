using System.Collections.Generic;
namespace Easy.EasyAsset
{
    public class EasyAssetBundleConfigInfo
    {
        public EasyAssetBundleType easyAssetBundleType;

        public List<string> assets = new List<string>();

        public List<string> packages = new List<string>();

        public List<string> groups = new List<string>();
    }
}