using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Easy.EasyAsset
{

    public class AnalyseAssetAttribute : Attribute
    {
        public string extension;

        public AnalyseAssetAttribute(string extension)
        {
            this.extension = extension;
        }
    }

    public interface IAnalyseAsset
    {
        List<string> GetDependencies(string assetPath);
    }

    public static class AnalyseAssetContext
    {
        private static Dictionary<string, IAnalyseAsset> allAnalyseAssets;

        static AnalyseAssetContext()
        {
            if(allAnalyseAssets != null)
            {
                allAnalyseAssets.Clear();
            }
            IEnumerable<IAnalyseAsset> list = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(IAnalyseAsset).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract && type.GetCustomAttribute<AnalyseAssetAttribute>() != null)
                .Select(Activator.CreateInstance)
                .Cast<IAnalyseAsset>();
            allAnalyseAssets = new Dictionary<string, IAnalyseAsset>();
            foreach (var analyseAsset in list)
            {
                AnalyseAssetAttribute attribute = analyseAsset.GetType().GetCustomAttribute<AnalyseAssetAttribute>();
                allAnalyseAssets.Add(attribute.extension, analyseAsset);
            }
        }

        public static List<string> GetDependencies(string assetPath)
        {
            string extension = Path.GetExtension(assetPath);
            if(allAnalyseAssets.ContainsKey(extension))
            {
                return allAnalyseAssets[extension].GetDependencies(assetPath);
            }
            return allAnalyseAssets[".*"].GetDependencies(assetPath);
        }
    }
}