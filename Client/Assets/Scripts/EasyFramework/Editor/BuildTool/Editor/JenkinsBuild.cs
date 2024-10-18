using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Easy;
using Easy.EasyAsset;
using Google.Android.AppBundle.Editor;
using Toolbox.Editor.Wizards;
using UnityEditor;
using UnityEngine;

namespace Easy
{

    public class JenkinsBuild : ToolboxWizard
    {
        private static string buildPath;

        private static string BuildPath
        {
            get
            {
                if (string.IsNullOrEmpty(buildPath))
                {
                    return EditorPrefs.GetString("buildPath", "./Build");
                }
                return buildPath;
            }
        }

        /// <summary>
        /// 自定义构建命令
        /// </summary>
        private static string[] _commandLineArgs;
        /// <summary>
        /// 自定义构建命令
        /// </summary>
        private static string[] GetCommandLineArgs()
        {
            if (_commandLineArgs != null)
            {
                string[] _tempArgs = _commandLineArgs;
                _commandLineArgs = null;
                return _tempArgs;
            }
            return System.Environment.GetCommandLineArgs();
        }
        /// <summary>
        /// 获取脚本传参
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetCommandParams(string methodName)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            bool isArg = false;
            foreach (string arg in GetCommandLineArgs())
            {
                if (isArg)
                {
                    if (arg.StartsWith("--"))
                    {
                        int splitIndex = arg.IndexOf("=", StringComparison.Ordinal);
                        if (splitIndex > 0)
                        {
                            args.Add(arg.Substring(2, splitIndex - 2), arg.Substring(splitIndex + 1));
                        }
                        else
                        {
                            args.Add(arg.Substring(2), "true");
                        }
                    }
                }
                else if (arg == methodName)
                {
                    isArg = true;
                }
            }

            return args;
        }

        /// <summary>
        /// 获取有效的scene文件
        /// </summary>
        private static string[] GetScenes()
        {
            string[] scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }
            return scenes;
        }

        private static void SetCommonBuildSettings(Dictionary<string, string> args, ref BuildPlayerOptions opt)
        {
            //构建方式
            BuildOptions options = BuildOptions.None;
            if(args.ContainsKey("BuildOptions"))
            {
                string[] buildOptions = args["BuildOptions"].Split('|');
                foreach(string buildOption in buildOptions)
                {
                    if(Enum.TryParse(buildOption,true, out BuildOptions v))
                    {
                        options |= v;
                    }
                }
            }
            opt.options = options;
            //是否开发模式
            EditorUserBuildSettings.development =  args.ContainsKey("development") ? bool.Parse(args["development"]) : false;
            //包名
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, args.ContainsKey("applicationIdentifier") ? args["applicationIdentifier"] : PlayerSettings.applicationIdentifier);
            //版本号
            PlayerSettings.bundleVersion = args.ContainsKey("version") ? args["version"] : PlayerSettings.bundleVersion;
        }

        /// <summary>
        /// 设置安卓通用设置
        /// </summary>
        private static void SetAndroidCustomBuildSettings(Dictionary<string, string> args, ref BuildPlayerOptions opt)
        {
            //Symbols
            EditorUserBuildSettings.androidCreateSymbols = args.ContainsKey("androidCreateSymbols") ? (AndroidCreateSymbols)Enum.Parse(typeof(AndroidCreateSymbols), args["androidCreateSymbols"]): EditorUserBuildSettings.androidCreateSymbols;
            //AppBundle
            EditorUserBuildSettings.buildAppBundle = args.ContainsKey("buildAppBundle") ? bool.Parse(args["buildAppBundle"]) : EditorUserBuildSettings.buildAppBundle;
            //AS工程
            EditorUserBuildSettings.exportAsGoogleAndroidProject = args.ContainsKey("exportAsGoogleAndroidProject") ? bool.Parse(args["exportAsGoogleAndroidProject"]) : EditorUserBuildSettings.exportAsGoogleAndroidProject;
            //version code
            PlayerSettings.Android.bundleVersionCode = args.ContainsKey("versionCode") ? int.Parse(args["versionCode"]) : PlayerSettings.Android.bundleVersionCode;
            //key密钥
            PlayerSettings.keystorePass = args.ContainsKey("keystorePass") ? args["keystorePass"] : PlayerSettings.keystorePass;
            PlayerSettings.keyaliasPass = args.ContainsKey("keyaliasPass") ? args["keyaliasPass"] : PlayerSettings.keyaliasPass;
            bool isDevelopment = args.ContainsKey("development") ? bool.Parse(args["development"]) : false;
            string name = isDevelopment ? "Release" : "Debug";
            string ext = EditorUserBuildSettings.exportAsGoogleAndroidProject? "_as" : EditorUserBuildSettings.buildAppBundle? "_aab.aab" : ".apk";
            opt.locationPathName = BuildPath + "/Android/"+ name + "/" + name +  ext;
        }

        /// <summary>
        /// 设置IOS通用设置
        /// </summary>
        /// <param name="methodName"></param>
        private static void SetIOSCustomBuildSettings(Dictionary<string, string> args, ref BuildPlayerOptions opt)
        {
            PlayerSettings.iOS.buildNumber = args.ContainsKey("versionCode") ? args["versionCode"] : PlayerSettings.iOS.buildNumber;
            bool isDevelopment = args.ContainsKey("development") ? bool.Parse(args["development"]) : false;
            opt.locationPathName = BuildPath + "/IOS/"+ (isDevelopment ?"Debug":"Release");
        }

        /// <summary>
        /// 设置IOS通用设置
        /// </summary>
        /// <param name="methodName"></param>
        private static void SetWinCustomBuildSettings(Dictionary<string, string> args, ref BuildPlayerOptions opt)
        {
            bool isDevelopment = args.ContainsKey("development") ? bool.Parse(args["development"]) : false;
            opt.locationPathName = BuildPath + "/StandaloneWindows64/"+ (isDevelopment ?"Debug":"Release") +"/Game.exe";
        }

        public static void BuildAsset(Dictionary<string, string> args, BuildTargetGroup buildTargetGroup, BuildTarget buildTarget)
        {
            bool isBuildAsset = args.ContainsKey("buildAsset") ? bool.Parse(args["buildAsset"]) : false;
            if (isBuildAsset)
            {

                BuildAssetBundleOptions buildAssetBuildOptions = BuildAssetBundleOptions.None;
                if(args.ContainsKey("buildAssetBundleOptions"))
                {
                    string[] buildOptions = args["buildAssetBundleOptions"].Split('|');
                    foreach(string buildOption in buildOptions)
                    {
                        if(Enum.TryParse(buildOption,true, out BuildAssetBundleOptions v))
                        {
                            buildAssetBuildOptions |= v;
                        }
                    }
                }

                CopyType copyType = CopyType.CopyVersionAndCatalog;
                if( args.ContainsKey("copyType") )
                {
                    Enum.TryParse(args["copyType"],true, out copyType);
                }

                try
                {
#if AA
                    AABuildTool.BuildAddressables(buildTargetGroup, buildTarget);
#else
                    GenerateContext.Instance.Reset();
                    GenerateContext.Instance.generateInfo.build_target = buildTarget;
                    GenerateContext.Instance.generateInfo.buildAssetBuildOptions = buildAssetBuildOptions;
                    GenerateContext.Instance.generateInfo.copyType = copyType;

                    var buildTaskPipeLine = AssetDatabase.LoadAssetAtPath<BuildTaskPipeLine>(EasyAssetEditorConst.EasyAssetBuildTaskPipleLinePath);
                    if (buildTaskPipeLine == null)
                    {
                        buildTaskPipeLine = ScriptableObject.CreateInstance<BuildTaskPipeLine>();
                        AssetDatabase.CreateAsset(buildTaskPipeLine, EasyAssetEditorConst.EasyAssetBuildTaskPipleLinePath);
                    }
                    buildTaskPipeLine.StartTask();
    #endif
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        [MenuItem("Easy/Jenkins/SetBuildPath")]
        static void ShowWindow()
        {
            buildPath = EditorUtility.OpenFolderPanel("Select Build Path", "", "");
            EditorPrefs.SetString("buildPath", buildPath);  
        }

        [MenuItem("Easy/Jenkins/Android/Build Release For Android", false)]
        public static void BuildAndroidReleaseProject()
        {
            _commandLineArgs = new string[]
            {
                "Easy.JenkinsBuild.BuildAndroidProject",
                "--develepment=false",
                "--buildAsset=true",
                "--androidCreateSymbolsZip=true",
                "--buildAppBundle=false",
                "--exportAsGoogleAndroidProject=false",
                "--BuildOptions=Development|AllowDebugging"
            };
            BuildAndroidProject();
        }


        /// <summary>
        /// 构建debug 版本
        /// </summary>
        /// 
        [MenuItem("Easy/Jenkins/Android/Build Debug For Android", false)]
        public static void BuildAndroidDebugProject()
        {
            _commandLineArgs = new string[]
            {
                "Easy.JenkinsBuild.BuildAndroidProject",
                "--develepment=true",
                "--buildAsset=true",
                "--androidCreateSymbolsZip=false",
                "--buildAppBundle=false",
                "--exportAsGoogleAndroidProject=false",
                "--BuildOptions=Development|AllowDebugging"
            };
            BuildAndroidProject();
        }

        /// <summary>
        /// 构建debug 版本
        /// </summary>
        /// 
        [MenuItem("Easy/Jenkins/Android/Build AndroidStudio Project", false)]
        public static void BuildAndroidStudioDebugProject()
        {
            _commandLineArgs = new string[]
            {
                "Easy.JenkinsBuild.BuildAndroidProject",
                "--develepment=true",
                "--buildAsset=true",
                "--androidCreateSymbolsZip=false",
                "--buildAppBundle=false",
                "--exportAsGoogleAndroidProject=true",
                "--BuildOptions=Development|AllowDebugging",
            };
            BuildAndroidProject();
        }

        /// <summary>
        /// 
        /// 构建android aab包
        /// </summary>
        [MenuItem("Easy/Jenkins/Android/Build Release For Android AAB", false)]
        public static void BuildAndroidReleaseAABProject()
        {
            _commandLineArgs = new string[]
            {
                "Easy.JenkinsBuild.BuildAndroidProject",
                "--develepment=false",
                "--buildAsset=true",
                "--androidCreateSymbolsZip=false",
                "--buildAppBundle=true",
                "--exportAsGoogleAndroidProject=false",
                "--PAD=true",
            };
            BuildAndroidProject();
        }

        public static void BuildAndroidProject()
        {
            Dictionary<string, string> args = GetCommandParams("Easy.JenkinsBuild.BuildAndroidProject");
            BuildPlayerOptions opt = new BuildPlayerOptions();
            SetCommonBuildSettings(args, ref opt);
            SetAndroidCustomBuildSettings(args, ref opt);
            BuildAsset(args,BuildTargetGroup.Android, BuildTarget.Android);
            opt.scenes = GetScenes();
            opt.target = BuildTarget.Android;
            opt.targetGroup = BuildTargetGroup.Android;

#if UNITY_ANDROID
            if (args.TryGetValue("PAD", out string value) && value.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                Bundletool.BuildBundle(opt, PAD.GenAssetPackConfig());
            }
            else
#endif
            {
                BuildPipeline.BuildPlayer(opt);
            }
        }

        /// <summary>
        /// ios不加按钮  走打包机脚本
        /// </summary>
        ///
        [MenuItem("Easy/Jenkins/Ios/Build Debug", false)]
        public static void BuildIosDebugProject()
        {
            _commandLineArgs = new string[]
            {
                "Easy.JenkinsBuild.BuildIosProject",
                "--develepment=false",
                "--buildAsset=true",
                "--BuildOptions=Development|AllowDebugging"
            };
            BuildIosProject();
        }

        /// <summary>
        /// ios不加按钮  走打包机脚本
        /// </summary>
        [MenuItem("Easy/Jenkins/Ios/Build Release", false)]
        public static void BuildIosReleaseProject()
        {
            _commandLineArgs = new string[]
            {
                "Easy.JenkinsBuild.BuildIosProject",
                "--develepment=false",
                "--buildAsset=true"
            };
            BuildIosProject();
        }

        public static void BuildIosProject()
        {
            Dictionary<string, string> args = GetCommandParams("Easy.JenkinsBuild.BuildIosProject");
            BuildPlayerOptions opt = new BuildPlayerOptions();
            SetCommonBuildSettings(args, ref opt);
            SetIOSCustomBuildSettings(args, ref opt);
            BuildAsset(args,BuildTargetGroup.iOS, BuildTarget.iOS);
            opt.scenes = GetScenes();
            opt.target = BuildTarget.iOS;
            BuildPipeline.BuildPlayer(opt);
        }

        /// <summary>
        /// ios不加按钮  走打包机脚本
        /// </summary>
        ///
        [MenuItem("Easy/Jenkins/Win/Build Debug", false)]
        public static void BuildWinDebugProject()
        {
            _commandLineArgs = new string[]
            {
                "Easy.JenkinsBuild.BuildWinProject",
                "--develepment=true",
                "--buildAsset=true",
                "--BuildOptions=Development|AllowDebugging"
            };
            BuildWinProject();
        }

        /// <summary>
        /// ios不加按钮  走打包机脚本
        /// </summary>
        [MenuItem("Easy/Jenkins/Win/Build Release", false)]
        public static void BuildWinReleaseProject()
        {
            _commandLineArgs = new string[]
            {
                "Easy.JenkinsBuild.BuildWinProject",
                "--develepment=false",
                "--buildAsset=true"
            };
            BuildWinProject();
        }

        public static void BuildWinProject()
        {
            Dictionary<string, string> args = GetCommandParams("Easy.JenkinsBuild.BuildWinProject");
            BuildPlayerOptions opt = new BuildPlayerOptions();
            SetCommonBuildSettings(args, ref opt);
            SetWinCustomBuildSettings(args, ref opt);
            BuildAsset(args,BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            opt.scenes = GetScenes();
            opt.target = BuildTarget.StandaloneWindows64;
            BuildPipeline.BuildPlayer(opt);
        }
    }

}