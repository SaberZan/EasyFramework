using System.Reflection.Emit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.ComponentModel;

namespace Easy.EasyAsset
{
    [System.Serializable]
    public struct GroupConfigInfo
    {
        [SerializeField]
        private string _groupName;

        public string groupName
        {
            set => _groupName = value;
            get => string.IsNullOrEmpty(_groupName)? string.Empty : "g_" + _groupName;
        }
        
        [BeginHorizontal]
        [Tooltip("打包策略")]
        [LabelWidth(80)]
        public PackType packType;
        
        
        [Tooltip("下载优先级")]
        [LabelWidth(80)]
        public DownloadPriority downloadPriorityType;

        [Tooltip("包所在位置")]
        [LabelWidth(80)]
        public Location location;

        [Tooltip("是否加密")]
        [LabelWidth(80)]
        public bool isEncrypt;


        [SerializeField]
        [Tooltip("是否原始资源")]
        [LabelWidth(80)]
        public bool isRaw;
        
        [Tooltip("捆绑资源不可拆分")]
        [LabelWidth(80)]
        [EndHorizontal]
        public bool isBind;

        [ReorderableList(ListStyle.Round, elementLabel: "资源", Foldable = true)]
        public List<UnityEngine.Object> assets;

    }
}