using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Easy
{
    public static class EasyLogger
    {
        private static int _index = 0;

		private static Thread _thread;

        private static StreamWriter _streamWriter;

        private static readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();

        private static readonly string _LOG_ROOT_PATH = Path.Combine(Application.persistentDataPath, "Log");

		private static readonly int _MAX_SIZE = 1024 * 1024 * 20;

        private static readonly int _MAX_LOG_DAYS = 3;

        public static List<string> openTags = new List<string>();

        public static bool IsLogOpen = true;

        public static bool IsWriteFile = false;

        public static bool IsUnityLog = false;


        [Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void Log(Func<string> tagFunc, Func<string> messageFun)
        {
            if(!IsLogOpen)
            { 
                return;
            }
            var tag = tagFunc();
            var message = messageFun();
            if(!openTags.Contains(tag)) 
            { 
                return;
            }
            Log(()=>$"tag: {tag}, messgae: {message}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void Log(Func<string> messageFun)
        {
            if(!IsLogOpen) 
            {
                return;
            }
            var message = messageFun();

            if(IsUnityLog)
            {
                UnityEngine.Debug.Log(message);
            }

            if(IsWriteFile)
            {
                if(_thread == null)
                {
                    _thread = new Thread(Write);
                    _thread.Start();
                }
                _logQueue.Enqueue($"I>{message}");
            }

        }

        [Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogWarning(Func<string> tagFunc, Func<string> messageFun)
        {
            if(!IsLogOpen)
            { 
                return;
            }
            var tag = tagFunc();
            var message = messageFun();
            if(!openTags.Contains(tag)) 
            { 
                return;
            }
            LogWarning(()=>$"tag: {tag}, messgae: {message}");
        }

		[Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogWarning(Func<string> messageFun)
        {
            if(!IsLogOpen)
            { 
                return;
            }
            var message = messageFun();

            if(IsUnityLog)
            {
                UnityEngine.Debug.LogWarning(message);
            }

            if(IsWriteFile)
            {
                if(_thread == null)
                {
                    _thread = new Thread(Write);
                    _thread.Start();
                }
                _logQueue.Enqueue($"I>{message}");
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogError(Func<string> tagFunc, Func<string> messageFun)
        {
            if(!IsLogOpen)
            { 
                return;
            }
            var tag = tagFunc();
            if(!openTags.Contains(tag)) 
            { 
                return;
            }
            var message = messageFun();
            LogError(()=>$"tag: {tag}, messgae: {message}");
        }

		[Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogError(Func<string> messageFun)
        {
            if(!IsLogOpen)
            { 
                return;
            }
            var message = messageFun();

            if(IsUnityLog)
            {
                UnityEngine.Debug.LogError(message);
            }

            if(IsWriteFile)
            {
                if(_thread == null)
                {
                    _thread = new Thread(Write);
                    _thread.Start();
                }
                _logQueue.Enqueue($"I>{message}");
            }
        }

		[Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogException(Exception message)
        {
            if(IsUnityLog)
            {
                UnityEngine.Debug.LogException(message);
            }

            if(IsWriteFile)
            {
                if(_thread == null)
                {
                    _thread = new Thread(Write);
                    _thread.Start();
                }
                _logQueue.Enqueue($"I>{message}");
            }
        }

        private static string GetFileName(DateTime dateTime)
        {
            _index++;
            string dataDir = dateTime.ToString("yyyy_MM_dd");
            string logDirPath = Path.Combine(_LOG_ROOT_PATH, dataDir);
            if (!Directory.Exists(logDirPath))
            {
                Directory.CreateDirectory(logDirPath);
            }
            string logFileName = $"{dateTime:yyMMdd_HHmmss}_{_index}.Log";
            return Path.Combine(logDirPath, logFileName);
        }

        private static string GetDataTimeLog(string log)
        {
            return $"[{DateTime.Now:yy-MM-dd HH:mm:ss}]>>>{log}";
        }

        public static void Write()
        {
			if (_streamWriter == null)
            {
				DateTime currentTime = DateTime.Now;
				RemoveOldLogs(currentTime);
				_streamWriter = new StreamWriter(GetFileName(currentTime), false, Encoding.UTF8) { AutoFlush = true };
            }
			else if (_streamWriter.BaseStream.Length >= _MAX_SIZE)
			{
				_streamWriter.Close();
				_streamWriter = null;
				DateTime currentTime = DateTime.Now;
				_streamWriter = new StreamWriter(GetFileName(currentTime), false, Encoding.UTF8) { AutoFlush = true };

			}

			while (true)
            {
                if (_logQueue.TryDequeue(out string log))
                {
					_streamWriter.WriteLine(log);
                }
                else
                {
                    Thread.Sleep(17);
                }
            }
        }

        private static void RemoveOldLogs(DateTime now)
        {
            HashSet<string> foldersToKeep = new HashSet<string>();
            for (int i = 0; i < _MAX_LOG_DAYS; i++)
            {
                DateTime current = now.AddDays(-i);
				string dataDir = current.ToString("yyyy_MM_dd");
                string folder = Path.Combine(_LOG_ROOT_PATH, dataDir);
                foldersToKeep.Add(folder);
            }
            if (Directory.Exists(_LOG_ROOT_PATH))
            {
                try
                {
                    string[] allLogDir = Directory.GetDirectories(_LOG_ROOT_PATH);
                    foreach (string dir in allLogDir)
                    {
                        if (!foldersToKeep.Contains(dir))
                        {
							Directory.Delete(dir, true);
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

    }
}