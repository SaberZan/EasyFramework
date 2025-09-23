using System;
using System.Collections.Generic;
using System.IO;
using Easy;
using UnityEditor;
using UnityEngine;

[Serializable]
public class KeyToAsset : ScriptableObject
{
    [Serializable]

    public class ExtraData
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public UnityEngine.Object Object;
    }


    [SerializeField]
    [ReorderableList(ListStyle.Round, elementLabel: "键2路径", Foldable = true)]
    public List<ExtraData> Data = new List<ExtraData>();


    public List<String> GetKeys(string path)
    {
        List<String> keys = new List<string>();
        foreach (var item in Data)
        {
            if (AssetDatabase.GetAssetPath(item.Object) == path)
            {
                keys.Add(item.Name);
            }
        }

        var dirPath = Path.GetDirectoryName(path);
        foreach (var item in Data)
        {
            var assetPath = AssetDatabase.GetAssetPath(item.Object);
            if (AssetDatabase.IsValidFolder(assetPath) && assetPath == dirPath)
            {
                keys.Add(item.Name);
            }
        }

        return keys;
    }

}