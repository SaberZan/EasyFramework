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

        private static List<string> openTags = new List<string>();


        [Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void Log(string tag, string message)
        {
            if(!openTags.Contains(tag)) return;
            Log($"tag: {tag}, messgae: {message}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void Log(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
#else
            if(_thread == null)
			{
				_thread = new Thread(Write);
				_thread.Start();
			}
			_logQueue.Enqueue($"I>{message}");
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogWarning(string tag, string message)
        {
            if(!openTags.Contains(tag)) return;
            LogWarning($"tag: {tag}, messgae: {message}");
        }

		[Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogWarning(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(message);
#else
            if(_thread == null)
			{
				_thread = new Thread(Write);
				_thread.Start();
			}
			_logQueue.Enqueue($"W>{message}");
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogError(string tag, string message)
        {
            if(!openTags.Contains(tag)) return;
            LogError($"tag: {tag}, messgae: {message}");
        }

		[Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogError(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(message);
#else
            if(_thread == null)
			{
				_thread = new Thread(Write);
				_thread.Start();
			}
			_logQueue.Enqueue($"E>{message}");
#endif
        }

		[Conditional("UNITY_EDITOR"), Conditional("ENABLE_LOG")]
        public static void LogException(Exception message)
        {
#if UNITY_EDITOR
			UnityEngine.Debug.LogException(message);
#else
			if(_thread == null)
			{
				_thread = new Thread(Write);
				_thread.Start();
			}
			_logQueue.Enqueue($"Exc>{message.ToString()}");
#endif
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