using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Toolbox.Editor.Wizards;

namespace Easy
{

    public class BuildHelper : ToolboxWizard
    {
#if AA
        private const string AAorAB = "Easy/ChangeToEasyAsset";
#else
        private const string AAorAB = "Easy/ChangeToAddressable";
#endif

        [MenuItem(AAorAB, false)]
        public static void SetAAorAB()
        {
            SymbolUtility.ToogleSymbol("AA",EditorUserBuildSettings.selectedBuildTargetGroup);
        }
    }

}