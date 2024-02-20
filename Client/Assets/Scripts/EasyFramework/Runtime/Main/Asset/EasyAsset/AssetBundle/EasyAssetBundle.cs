using System;

namespace Easy.EasyAsset
{
    public abstract class EasyAssetBundle
    {
        public EasyAssetsLoader loader;

        public int index;

        protected EasyAssetBundle(EasyAssetsLoader loader, int index)
        {
            this.loader = loader;
            this.index = index;
        }

        public abstract void Unload(bool value);
    }
}