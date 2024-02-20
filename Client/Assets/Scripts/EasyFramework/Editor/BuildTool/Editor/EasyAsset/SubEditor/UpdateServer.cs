#if !AA

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;

namespace Easy.EasyAsset
{
    public class UpdateServer : ScriptableObject
    {
        private static CommandRunner commandRunner;

        public int port = 9999;

        [EditorButton(nameof(HotUpdateTestServer), EasyAssetEditorConst.HOT_UPDATE_TEST_SERVER)]
        [Directory]
        public string dir = "../Assets/AssetBundles/Output";
        public void HotUpdateTestServer()
        {
            commandRunner?.Close();
#if UNITY_EDITOR_WIN
            string executablePath = "powershell.exe";
#elif UNITY_EDITOR_OSX
            string executablePath = "/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
#else
            string executablePath = "/bin/bash";
#endif
            commandRunner = new CommandRunner(executablePath, Application.dataPath + "/../HotUpdate/", true, true, false);
            commandRunner.Run($"ts-node ./App.ts {port} {dir}/",(object sender, DataReceivedEventArgs e)=>{
                if (!String.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.Log(e.Data);
                }
            });
        }
        
    }
}

#endif