#if AA

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;


public class AABuildTool
{

    public static void BuildAddressables(BuildTargetGroup buildTargetGroup, BuildTarget target)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, target);
        BuildContent();
    }

    public static void UpdateAddressables(BuildTargetGroup buildTargetGroup, BuildTarget target)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, target);
        BuildUpdate();
    }

    public static void CheckForUpdateContent()
    {
        //与上次打包做资源对比
        string buildPath = ContentUpdateScript.GetContentStateDataPath(false);
        var m_Settings = AddressableAssetSettingsDefaultObject.Settings;
        List<AddressableAssetEntry> entrys = ContentUpdateScript.GatherModifiedEntries(m_Settings, buildPath);
        if (entrys.Count == 0) return;
        StringBuilder sbuider = new StringBuilder();
        sbuider.AppendLine("Need Update Assets:");
        foreach (var _ in entrys)
        {
            sbuider.AppendLine(_.address);
        }
        Debug.Log(sbuider.ToString());

        //将被修改过的资源单独分组
        var groupName = string.Format("UpdateGroup_{0}", DateTime.Now.ToString("yyyyMMdd"));
        ContentUpdateScript.CreateContentUpdateGroup(m_Settings, entrys, groupName);
    }

    private static void BuildContent()
    {
        AddressableAssetSettings.BuildPlayerContent();
    }

    private static void BuildUpdate()
    {
        var path = ContentUpdateScript.GetContentStateDataPath(false);
        var m_Settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressablesPlayerBuildResult result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
        Debug.Log("BuildFinish path = " + m_Settings.RemoteCatalogBuildPath.GetValue(m_Settings));
    }

    public static string LogAAPath()
    {
        string str = "";
        str += "BuildPath = " + Addressables.BuildPath + "\n";
        str += "PlayerBuildDataPath = " + Addressables.PlayerBuildDataPath + "\n";
        str += "RemoteCatalogBuildPath = " + AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.GetValue(AddressableAssetSettingsDefaultObject.Settings) + "\n";
        return str;
    }

    public static string GetBuildPath()
    {
        return Addressables.BuildPath;
    }

    public static string GetPlayerBuildDataPath()
    {
        return Addressables.PlayerBuildDataPath;
    }

    public static string GetHotUpdatePath()
    {
        return AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.GetValue(AddressableAssetSettingsDefaultObject.Settings);
    }
}

#endif