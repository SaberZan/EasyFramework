using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Easy.EasyAsset
{
    [Serializable]
    public class GenerateContext
    {
        public PackageConfig packageConfig;

        public KeyToAsset keyToAsset;

        public GenerateInfo generateInfo;

        public Version version;

        public Catalogs catalogs;

        public Dictionary<string, EasyAssetConfigInfo> allAssets;

        public Dictionary<string, EasyAssetBundleConfigInfo> easyAssetBundleConfigInfos;
        
        private static GenerateContext _context;

        public static GenerateContext Instance
        {
            get
            {
                if(_context == null)
                {
                    _context = new GenerateContext();
                }
                return _context;
            }
        }

        public void Reset()
        {
            packageConfig = AssetDatabase.LoadAssetAtPath<PackageConfig>(EasyAssetEditorConst.EasyAssetConfigPath);
            keyToAsset = AssetDatabase.LoadAssetAtPath<KeyToAsset>(EasyAssetEditorConst.KeyToAssetPath);
            generateInfo = AssetDatabase.LoadAssetAtPath<GenerateInfo>(EasyAssetEditorConst.GenerateInfoPath);
            allAssets = new Dictionary<string, EasyAssetConfigInfo>();
            easyAssetBundleConfigInfos = new Dictionary<string, EasyAssetBundleConfigInfo>();
        }
    }
}