using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Easy;
using Easy.EasyAsset;
using Toolbox.Editor.Wizards;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{

    public class EasyFrameworkEditor : ToolboxWizard
    {
        [EditorButton(nameof(TypesAssembliesChanged))]
        public List<string> typesAssemblies = new List<string>();

        [SerializeReference]
        [ReferencePicker(ParentType = typeof(EasyConfig))]
        [ReorderableList(ListStyle.Lined, "EasyConfigs", true, false)]
        public List<EasyConfig> easyConfigs = new List<EasyConfig>();

        private List<string> easyKeys = new List<string>();
 
        private Dictionary<string, Type> allConfigTypes = new Dictionary<string, Type>();


        [MenuItem("Easy/EasyFrameworkConfig", false)]
        static void OpenWindow()
        {
            var window = ToolboxWizard.DisplayWizard<EasyFrameworkEditor>("EasyFrameworkEditor");
            Array.ForEach(AppDomain.CurrentDomain.GetAssemblies(), assembly =>
            {
                Type[] types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (t.IsAbstract)
                    {
                        continue;
                    }
                    if (t.IsInterface)
                    {
                        continue;
                    }
                    if (typeof(Easy.EasyConfig).IsAssignableFrom(t))
                    {
                        window.allConfigTypes.Add(t.Name,t);
                    }
                }
            });

            string configPath = Path.Combine(Application.dataPath, "Resources", EasyFrameworkConfig.SETTINGS_NAME + ".json");
            if (File.Exists(configPath))
            {
                EasyFrameworkConfig config = JsonUtility.FromJson<EasyFrameworkConfig>(File.ReadAllText(configPath));
                window.typesAssemblies = config.typesAssemblies;
                for(int i = 0; i < config.keys.Count; ++i)
                {
                    string typeName = config.keys[i];
                    Type type = window.allConfigTypes[typeName];
                    var easyConfig = (EasyConfig)JsonUtility.FromJson(config.values[i], type);
                    window.easyKeys.Add(typeName);
                    window.easyConfigs.Add(easyConfig);
                }
            }
            window.Show();
        }

        public void TypesAssembliesChanged()
        {
            foreach (var item in allConfigTypes)
            {
                if (!easyKeys.Contains(item.Key))
                {
                    easyKeys.Add(item.Key);
                    easyConfigs.Add((Easy.EasyConfig)item.Value.Assembly.CreateInstance(item.Value.FullName ?? string.Empty));
                }
            }
        }

        protected override bool HandleCreateButton()
        {
            return GUILayout.Button("Save", GUILayout.MinWidth(100));
        }

        protected override void OnWizardCreate()
        {
            string configPath = Path.Combine(Application.dataPath, "Resources", EasyFrameworkConfig.SETTINGS_NAME + ".json");
            EasyFrameworkConfig config = new EasyFrameworkConfig();
            config.typesAssemblies = typesAssemblies;
            config.keys = easyKeys;
            for(int i = 0; i < config.keys.Count; ++i)
            {
                config.values.Add(JsonUtility.ToJson(easyConfigs[i]));
            }
            File.WriteAllText(configPath, JsonUtility.ToJson(config));
        }
    }
}