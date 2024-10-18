#if !AA

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;

namespace Easy.EasyAsset
{
    [Serializable]
    public class BuildTaskPipeLine : ScriptableObject
    {
        [SerializeReference, ReferencePicker]
        public List<IBuildTask> buildTasks = new List<IBuildTask>()
        {
            new ShaderVariantTask(),
            new HyBridCLRTask(),
            new AnalyseAssetsTask(),
            new GenAssetBundleAssetTask(),
            new GenUnityAssetBundleTask(),
            new GenRawAssetBundleTask(),
            new GenCatalogsAndVersionTask(),
            new OutputTask(),
            new CopyToStreamAssets()
        };
        
        [EditorButton(nameof(ResetTask))]
        [Hide]
        public int reset;
        public void ResetTask()
        {
            buildTasks = new List<IBuildTask>()
            {
                new ShaderVariantTask(),
                new HyBridCLRTask(),
                new AnalyseAssetsTask(),
                new GenAssetBundleAssetTask(),
                new GenUnityAssetBundleTask(),
                new GenRawAssetBundleTask(),
                new GenCatalogsAndVersionTask(),
                new OutputTask(),
                new CopyToStreamAssets()
            };
        }

        [EditorButton(nameof(StartTask))]
        [Hide]
        public int start;
        public async void StartTask()
        {
            GenerateContext.Instance.Reset();
            for (int i = 0; i < buildTasks.Count; i++)
            {
                EditorUtility.DisplayProgressBar(buildTasks[i].BuildName(), "Start", 0);
                var watch = Stopwatch.StartNew();
                BuildResult result = await buildTasks[i].Run(GenerateContext.Instance);
                if(result == BuildResult.Fail)
                {
                    break;
                }
                watch.Stop();
                UnityEngine.Debug.Log(buildTasks[i].BuildName() + " 耗时 = " + watch.Elapsed);
                EditorUtility.DisplayProgressBar(buildTasks[i].BuildName(), "End", 1);
            }
            EditorUtility.ClearProgressBar();
        }
        
    }
}

#endif