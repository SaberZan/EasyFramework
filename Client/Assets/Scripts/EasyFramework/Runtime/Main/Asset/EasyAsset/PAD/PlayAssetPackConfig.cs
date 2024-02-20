#if UNITY_ANDROID

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayAssetPackConfigInfo
{
    [SerializeField]
    public string assetPackName;

    [SerializeField]
    public List<string> assetBundleNames = new List<string>();

    [SerializeField]
    public long size;
}


[Serializable]
public class PlayAssetPackConfig
{
    public List<PlayAssetPackConfigInfo> configInfos = new List<PlayAssetPackConfigInfo>();
}

#endif
