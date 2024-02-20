#if AA

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Easy.AA
{
    public class AAEditorWindow : OdinEditorWindow
    {
        private const string Build_AA = "生成AA包";
        private const string Update_AA = "生成更新AA包";
        private const string Generate_AA_UpdateContent = "生成差量包";
        private const string Open_Addressables_Group = "打开Addressables Group窗口";
        private const string Open_Addressables_Settings = "打开Addressables Settings窗口";
        private const string Open_Build_Path = "打开构建路径";
        private const string Open_Build_Data_Path = "打开构建数据路径";
        private const string Open_HotUpdate_Path = "打开热更路径";

        [MenuItem("Easy/AAEditor", false)]
        static void OpenWindow()
        {
            var window = GetWindow<AAEditorWindow>(false, "AAEditor");
            window.minSize = new Vector2(800, 600);
            window.AAPath = AABuildTool.LogAAPath();
            window.Show();
        }

        [DisplayAsString(Overflow = false)]
        public string AAPath;

        [Button(ButtonSizes.Large, Name = Open_Addressables_Group)]
        public void OpenAddressableAssetsGroupWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
        }

        [Button(ButtonSizes.Large, Name = Open_Addressables_Settings)]
        public void OpenAddressableAssetsSettingsWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Settings");
        }

        [Button(Open_Build_Path, Style = ButtonStyle.Box)]
        public void OpenBuildPath()
        {
            QuickOpenFolder.OpenExplorerFolder(AABuildTool.GetBuildPath());
        }

        [Button(Open_Build_Data_Path, Style = ButtonStyle.Box)]
        public void OpenPlayerBuildDataPath()
        {
            QuickOpenFolder.OpenExplorerFolder(AABuildTool.GetPlayerBuildDataPath());
        }

        [Button(Open_HotUpdate_Path, Style = ButtonStyle.Box)]
        public void OpenHotUpdatePath()
        {
            QuickOpenFolder.OpenExplorerFolder(AABuildTool.GetHotUpdatePath());
        }

        [Button(Build_AA, Style = ButtonStyle.Box)]
        public void BuildAA(BuildTargetGroup buildTargetGroup, BuildTarget target)
        {
            if (buildTargetGroup == 0)
            {
                buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            }
            if(target == 0)
            {
                target = EditorUserBuildSettings.activeBuildTarget;
            }
            AABuildTool.BuildAddressables(buildTargetGroup, target);
        }

        [Button(Update_AA, Style = ButtonStyle.Box)]
        public void UpdateAA(BuildTargetGroup buildTargetGroup, BuildTarget target)
        {
            if (buildTargetGroup == 0)
            {
                buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            }
            if (target == 0)
            {
                target = EditorUserBuildSettings.activeBuildTarget;
            }
            AABuildTool.UpdateAddressables(buildTargetGroup, target);
        }

        [Button(Generate_AA_UpdateContent, Style = ButtonStyle.Box)]
        public void CheckForUpdateContent()
        {
            AABuildTool.CheckForUpdateContent();
        }

    }
}

#endif