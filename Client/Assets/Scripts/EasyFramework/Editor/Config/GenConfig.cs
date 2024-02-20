
    using Easy;
    using UnityEditor;
    using UnityEngine;
    public class GenConfig: EditorWindow
    {
        private const string Match3_GenConfig = "Easy/GenConfig";

        [MenuItem(Match3_GenConfig, false)]
        public static void GenExcel()
        {
            CommandRunner commandRunner = new CommandRunner(Application.dataPath + "/../../Mod/gen.bat", Application.dataPath + "/../../Mod/", true, true, false);
            string msg = commandRunner.Run("");
            Debug.Log(msg);
        }
    }

