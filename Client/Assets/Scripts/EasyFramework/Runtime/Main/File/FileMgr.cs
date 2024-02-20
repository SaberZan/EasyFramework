using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace Easy
{

    /// <summary>
    /// 基础系统，需要预先加载
    /// 文件管理器
    /// 读取、保存指定文件
    /// </summary>
    [PreInit]
    [OrderIndex((int)PreInitOrderIndexEnum.FileMgr)]
    public class FileMgr : Singleton<FileMgr>
    {
        
        public override void Init(InitCompleteCallback complete)
        {
            complete.Invoke(true);
        }

        public override void BeforeRestart()
        {

        }
        
        /// <summary>
        /// 保存数据到文件中
        /// </summary>
        /// <param name="filename">文件名（绝对路径）</param>
        /// <param name="datastring">待保存的数据</param>
        public void SaveDataToFile(string filename, string datastring)
        {
            //Debuger.Log(filename);
            using (StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                sw.WriteLine(datastring);
                sw.Flush();
            }
        }

        /// <summary>
        /// 保存数据到文件中
        /// </summary>
        /// <param name="filename">文件名（绝对路径）</param>
        /// <param name="data">待保存的数据</param>
        /// <param name="size">保存的大小</param>
        public void SaveDataToFile(string filename, byte[] data, long size)
        {
            MemoryStream memory = new MemoryStream(data);
            byte[] buffer = new byte[size];
            FileStream file = File.Open(filename, FileMode.OpenOrCreate);
            int readBytes;
            while ((readBytes = memory.Read(buffer, 0, buffer.Length)) > 0)
            {
                file.Write(buffer, 0, readBytes);
            }
            file.Close();
        }
        
        /// <summary>
        /// 获取目标类型对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="fullpath">文件路径</param>
        /// <param name="isStreamingAsset">是否在streaming文件夹内</param>
        /// <returns>目标对象</returns>
        public T GetTargetClassObject<T>(string fullpath, bool isStreamingAsset = false, bool isEncrypt = true)
            where T : class
        {
            EasyLogger.LogWarning("EasyFrameWork",$"********** {isStreamingAsset} **********");
            byte[] encryptContent;
            if (isStreamingAsset)
            {
                encryptContent = LoadStreamingAssetFileSync(fullpath);
            }
            else
            {
                if (!File.Exists(fullpath))
                {
                    EasyLogger.LogWarning("EasyFrameWork", $"***** Need Load File Lost {fullpath}*****");
                    return null;
                }
                encryptContent = File.ReadAllBytes(fullpath);
            }
            if(isEncrypt)
            {
                XOREncryption.DecryptData(encryptContent, 0, -1, XOREncryption.DEFAULT_ENCRYPT_KEY, encryptContent.Length);
            }
            string jsonStr = Encoding.UTF8.GetString(encryptContent);
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">类型模板</typeparam>
        /// <param name="path">文件存储路径</param>
        /// <param name="classObj">类对象</param>
        public void SetTargetClassObjectToFile<T>(string path, T classObj, bool isEncrypt = true)
            where T : class
        {
            string jsonStr = JsonConvert.SerializeObject(classObj);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonStr);
            if(isEncrypt)
            {
                XOREncryption.EncryptData(buffer, 0, -1, XOREncryption.DEFAULT_ENCRYPT_KEY, buffer.Length);
            }
            File.WriteAllBytes(path, buffer);
        }
        
        /// <summary>
        /// 同步加载本地StreamingAsset内的文件
        /// </summary>
        /// <param name="filePath">全路径文件名</param>
        /// <returns></returns>
        public byte[] LoadStreamingAssetFileSync(string filePath)
        {
            EasyLogger.Log("EasyFrameWork", $"LoadStreamingAssetFileSync : Loading file name is {filePath}");

            byte[] databuffer;


#if UNITY_ANDROID
            //UnityWebRequest request = new UnityWebRequest(new Uri(filePath));
            //request.downloadHandler = new DownloadHandlerBuffer();
            //UnityWebRequestAsyncOperation ao = request.SendWebRequest();
            //while (!request.isDone || !ao.isDone)
            //{

            //}
            //databuffer = request.downloadHandler.data;

            AndroidFileStream androidFileStream = new AndroidFileStream(filePath);
            databuffer = new byte[androidFileStream.Length];
            androidFileStream.Read(databuffer, 0, databuffer.Length);
#else
            databuffer = File.ReadAllBytes(filePath);
#endif
            return databuffer;
        }
    }
}