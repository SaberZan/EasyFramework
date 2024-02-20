#if !AA

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace Easy.EasyAsset
{

    [Serializable]
    public class LoadType : ScriptableObject
    {

        public void OnEnable()
        {
            loadAssetType = SymbolUtility.IsDefinedSymbol("AB_LOAD_BUNDLE", EditorUserBuildSettings.selectedBuildTargetGroup) ? EasyAssetEditorConst.LOAD_ASSET_TYPE_BUNDLE : EasyAssetEditorConst.LOAD_ASSET_TYPE_ASSET_DATA_BASE;
        }

        [Label("º”‘ÿ¿‡–Õ", skinStyle: SkinStyle.Box)]
        [DynamicHelp(nameof(loadAssetType))]
        [EditorButton(nameof(SetAssetLoadType), activityType: ButtonActivityType.OnEditMode)]
        [Hide]
        public string loadAssetType = "";

        public void SetAssetLoadType()
        {
            loadAssetType = SymbolUtility.ToogleSymbol("AB_LOAD_BUNDLE", EditorUserBuildSettings.selectedBuildTargetGroup) ? EasyAssetEditorConst.LOAD_ASSET_TYPE_BUNDLE : EasyAssetEditorConst.LOAD_ASSET_TYPE_ASSET_DATA_BASE;   
        }
    }
}

#endif