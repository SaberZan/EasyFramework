using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor.Build.Content;

namespace Easy.EasyAsset
{
    public class OutputTask : IBuildTask
    {
        public string BuildName()
        {
            return "加密Bundle";
        }

        public BuildResult Run(GenerateContext context)
        {
            if (Directory.Exists(context.generateInfo.OutputPath))
                Directory.Delete(context.generateInfo.OutputPath, true);
            Directory.CreateDirectory(context.generateInfo.OutputPath);

            foreach (var abInfo in context.catalogs.allEasyAssetBundleInfos)
            {
                string abOriginPath = context.generateInfo.OriginPath + abInfo.md5;
                Debug.Log("output == " + abInfo.md5);
                EditorUtility.DisplayProgressBar("打包AB包", "加密: " + abInfo.md5, 0);
                byte[] buffer = File.ReadAllBytes(abOriginPath);
                if(abInfo.isEncrypt)
                {
                    XOREncryption.EncryptData(buffer, 0, -1, XOREncryption.DEFAULT_ENCRYPT_KEY, buffer.Length);
                }
                abInfo.md5 = MD5Utility.GetMd5Hash(buffer);

                string locationDir = context.generateInfo.OutputPath + abInfo.location.ToString();
                if (!Directory.Exists(locationDir))
                    Directory.CreateDirectory(locationDir);
                string downloadTypeDir = Path.Combine(locationDir, abInfo.abDownloadPriority.ToString());
                if (!Directory.Exists(downloadTypeDir))
                    Directory.CreateDirectory(downloadTypeDir);
                List<string> packages = abInfo.packages;
                packages.Sort();
                string packageDir = Path.Combine(downloadTypeDir, string.Join("_", packages));
                if (!Directory.Exists(packageDir))
                    Directory.CreateDirectory(packageDir);
                string abEncryptPath = packageDir + "/" + abInfo.md5;
                File.WriteAllBytes(abEncryptPath, buffer);
            }
            
            string catalogsJsonStr = JsonConvert.SerializeObject(context.catalogs);
            byte[] catalogsBuffer = Encoding.UTF8.GetBytes(catalogsJsonStr);
            XOREncryption.EncryptData(catalogsBuffer, 0, -1, XOREncryption.DEFAULT_ENCRYPT_KEY, catalogsBuffer.Length);
            File.WriteAllBytes(context.generateInfo.OutputPath + "catalogs.txt", catalogsBuffer);

            string versionJsonStr = JsonConvert.SerializeObject(context.version);
            byte[] versionBuffer = Encoding.UTF8.GetBytes(versionJsonStr);
            XOREncryption.EncryptData(versionBuffer, 0, -1, XOREncryption.DEFAULT_ENCRYPT_KEY, versionBuffer.Length);
            File.WriteAllBytes(context.generateInfo.OutputPath + "version.txt", versionBuffer);

            return BuildResult.Success;
        }
    }
}