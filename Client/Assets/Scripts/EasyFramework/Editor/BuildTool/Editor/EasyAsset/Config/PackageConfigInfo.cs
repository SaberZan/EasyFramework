using System.Security.AccessControl;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Easy.EasyAsset
{
    [System.Serializable]
    public struct PackageConfigInfo
    {
        public string _packageName;

        public string packageName 
        {
            set => _packageName = value;
            get => string.IsNullOrEmpty(_packageName)? string.Empty : "p_" + _packageName;
        }

        [ReorderableList(ListStyle.Round, elementLabel: "组", Foldable = true)]
        public List<GroupConfigInfo> groups;
    }
}