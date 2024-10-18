using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Debug = UnityEngine.Debug;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace Easy.EasyAsset
{
    public class ShaderVariantTask : IBuildTask
    {
        private readonly Stopwatch _elapsedTime = new Stopwatch();
        private string ShaderVariantCollectionPath = "Assets/ShaderVariantCollection.shadervariants";
        private int WaitTimeBeforeSave = 1000;
        private List<Material> materials = new List<Material>();
        private List<GameObject> _allSpheres = new List<GameObject>(1000);

        public string BuildName()
        {
            return "Shader收集";
        }

        public async Task<BuildResult> Run(GenerateContext context)
        {
            EditorApplication.update += EditorUpdate;

            materials = new List<Material>();
            HashSet<string> dic = new HashSet<string>();

            var _packageConfig = context.packageConfig;
            if (_packageConfig == null || _packageConfig.packageInfos.Count == 0)
            {
                return BuildResult.Fail;
            }
            foreach (PackageConfigInfo packageInfo in _packageConfig.packageInfos)
            {
                foreach (GroupConfigInfo abGroupInfo in packageInfo.groups)
                {
                    string groupName = abGroupInfo.groupName;
                    foreach (UnityEngine.Object asset in abGroupInfo.assets)
                    {
                        string path = AssetDatabase.GetAssetPath(asset);
                        if (Directory.Exists(path))
                        {
                            string directory = new DirectoryInfo(path).Name;
                            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                            foreach (string file in files)
                            {
                                string assetPath = file;
                                if(!dic.Contains(assetPath))
                                {
                                    dic.Add(assetPath);
                                }
                                string[] depends = AssetDatabase.GetDependencies(assetPath);
                                foreach (var item in depends)
                                {
                                    assetPath = item;
                                    if(!dic.Contains(assetPath))
                                    {
                                        dic.Add(assetPath);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string assetPath = path;
                            if(!dic.Contains(assetPath))
                            {
                                dic.Add(assetPath);
                            }
                            string[] depends = AssetDatabase.GetDependencies(assetPath);
                            foreach (var item in depends)
                            {
                                assetPath = item;
                                if(!dic.Contains(assetPath))
                                {
                                    dic.Add(assetPath);
                                }
                            }
                        }
                    }
                } 
            }

            
            var shaderDict = new Dictionary<Shader, List<Material>>();
            foreach (var assetPath in dic)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (material != null)
                {
                    if (material.shader != null)
                    {
                        if (!shaderDict.ContainsKey(material.shader))
                        {
                            shaderDict.Add(material.shader, new List<Material>());
                        }

                        if (!shaderDict[material.shader].Contains(material))
                        {
                            shaderDict[material.shader].Add(material);
                        }
                    }

                    if (!materials.Contains(material))
                    {
                        materials.Add(material);
                    }
                }
            }

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            InvokeInternalStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
            Debug.Log(InvokeInternalStaticMethod(typeof(ShaderUtil),"GetCurrentShaderVariantCollectionVariantCount"));
            _elapsedTime.Stop();
            _elapsedTime.Reset();
            _elapsedTime.Start();

            while(materials.Count > 0)
            {
                await Task.Yield();
            }

            InvokeInternalStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", ShaderVariantCollectionPath);
            Debug.Log(InvokeInternalStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount"));
            
            DestroyAllSpheres();

            var sb = new System.Text.StringBuilder();
            foreach (var kvp in shaderDict)
            {
                sb.AppendLine(kvp.Key + " " + kvp.Value.Count + " times");

                if (kvp.Value.Count <= 5)
                {
                    Debug.LogWarning("Shader: " + kvp.Key.name, kvp.Key);

                    foreach (var m in kvp.Value)
                    {
                        Debug.Log(AssetDatabase.GetAssetPath(m), m);
                    }
                }
            }
            Debug.Log(sb.ToString());

            EditorApplication.update -= EditorUpdate;
            return BuildResult.Success;
        }

        private void EditorUpdate()
        {
            if (materials.Count > 0 && _elapsedTime.ElapsedMilliseconds >= WaitTimeBeforeSave)
            {
                _elapsedTime.Stop();
                _elapsedTime.Reset();
                DestroyAllSpheres();
                List<Material> execMaterials = materials.GetRange(0, math.min(1000, materials.Count));
                materials.RemoveRange(0, execMaterials.Count);
                ProcessMaterials(execMaterials);
            }
        }

        private void ProcessMaterials(List<Material> materials)
        {
            int totalMaterials = materials.Count;
            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("Main Camera didn't exist");
                return;
            }

            float aspect = camera.aspect;
            float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
            float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
            float halfHeight = Mathf.CeilToInt(height / 2f);
            float halfWidth = Mathf.CeilToInt(width / 2f);
            camera.orthographic = true;
            camera.orthographicSize = halfHeight;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            Selection.activeGameObject = camera.gameObject;
            EditorApplication.ExecuteMenuItem("GameObject/Align View to Selected");

            int xMax = (int) (width - 1);

            int x = 0;
            int y = 0;

            for (int i = 0; i < materials.Count; i++)
            {
                var material = materials[i];

                var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
                CreateSphere(material, position, x, y, i);

                if (x == xMax)
                {
                    x = 0;
                    y++;
                }
                else
                {
                    x++;
                }
            }
        }

        private void CreateSphere(Material material, Vector3 position, int x, int y, int index)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.GetComponent<Renderer>().material = material;
            go.transform.position = position;
            go.name = string.Format("Sphere_{0}|{1}_{2}|{3}", index, x, y, material.name);
            _allSpheres.Add(go);
        }

        private void DestroyAllSpheres()
		{
			foreach(var go in _allSpheres)
			{
				GameObject.DestroyImmediate(go);
			}
			_allSpheres.Clear();
			// 尝试释放编辑器加载的资源
			EditorUtility.UnloadUnusedAssetsImmediate(true);
		}

        private object InvokeInternalStaticMethod(System.Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
            {
                Debug.LogError(string.Format("{0} method didn't exist", method));
                return null;
            }
            return methodInfo.Invoke(null, parameters);
        }


    }
}
