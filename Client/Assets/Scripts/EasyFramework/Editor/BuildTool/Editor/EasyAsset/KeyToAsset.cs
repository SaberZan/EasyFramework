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

    public class KeyData
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public UnityEngine.Object Object;
    }


    [SerializeField]
    [ReorderableList(ListStyle.Round, elementLabel: "键2路径", Foldable = true)]
    public List<KeyData> Data = new List<KeyData>();


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

        var dir = Directory.GetParent(path);
        while (dir != null)
        {
            foreach (var item in Data)
            {
                var assetPath = AssetDatabase.GetAssetPath(item.Object);
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    var assetDir = new DirectoryInfo(assetPath);
                    if (dir.FullName == assetDir.FullName)
                    {
                        keys.Add(item.Name);
                    }
                }
            }
            dir = dir.Parent; 
        }
        return keys;
    }

}