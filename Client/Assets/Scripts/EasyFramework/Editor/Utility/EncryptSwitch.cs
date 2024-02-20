using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Easy
{

    public class EncryptSwitch : EditorWindow
    {
#if ENCRYPT
        private const string EncryptStr = "Easy/切换到不加密";
#else
        private const string EncryptStr = "Easy/切换到加密";
#endif

        [MenuItem(EncryptStr, false)]
        public static void SetEncrypt()
        {
            SymbolUtility.ToogleSymbol("ENCRYPT",EditorUserBuildSettings.selectedBuildTargetGroup);
        }
    }

}