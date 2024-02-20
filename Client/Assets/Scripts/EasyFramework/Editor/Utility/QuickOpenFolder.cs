using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Easy
{

    public class QuickOpenFolder : EditorWindow
    {
        private const string Open_Folder_Assets = "Easy/Open Folder/AssetDataPath";
        private const string Open_Folder_Persistent = "Easy/Open Folder/PersistentDataPath";
        private const string Open_Folder_TemporaryCache = "Easy/Open Folder/TemporaryCachePath";
        private const string Open_Folder_StreamingAssets = "Easy/Open Folder/StreamingAssetsPath";
        
        [MenuItem(Open_Folder_Assets, false)]
        public static void OpenAssetsDataFolder()
        {
            OpenExplorerFolder(Application.dataPath);
        }
        [MenuItem(Open_Folder_Persistent, false)]
        public static void OpenPersistentDataFolder()
        {
            OpenExplorerFolder(Application.persistentDataPath);
        }
        [MenuItem(Open_Folder_TemporaryCache, false)]
        public static void OpenTemporaryCacheFolder()
        {
            OpenExplorerFolder(Application.temporaryCachePath);
        }
        [MenuItem(Open_Folder_StreamingAssets, false)]
        public static void OpenStreamingAssetsFolder()
        {
            OpenExplorerFolder(Application.streamingAssetsPath);
        }
        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="folderPath"></param>
        public static void OpenExplorerFolder(string folderPath)
        {
            System.Diagnostics.Process open = new System.Diagnostics.Process();
            open.StartInfo.FileName = "explorer";
            open.StartInfo.Arguments = @"/e /root," + folderPath.Replace("/", "\\");
            open.Start(); ;
        }
    }

}