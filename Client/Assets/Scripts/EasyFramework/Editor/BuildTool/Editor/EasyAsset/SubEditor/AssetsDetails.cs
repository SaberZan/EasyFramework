#if !AA

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;

namespace Easy.EasyAsset
{
    public class AssetsDetails : ScriptableObject
    {
        public Dictionary<string, EasyAssetConfigInfo> allAssets;

        public Dictionary<string, EasyAssetBundleConfigInfo> easyAssetBundleConfigInfos;
    }
}

#endif