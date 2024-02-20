using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Easy
{

    public static class ListExtension
    {
        static public T Pop<T>(this List<T> list)
        {
            T result = default(T);
            int index = list.Count - 1;
            if (index >= 0)
            {
                result = list[index];
                list.RemoveAt(index);
                return result;
            }
            return result;
        }

        static public void Push<T>(this List<T> list, T t)
        {
            list.Add(t);
        }

        static public T Dequeue<T>(this List<T> list)
        {
            T result = default(T);
            if (list.Count > 0)
            {
                result = list[0];
                list.RemoveAt(0);
                return result;
            }
            return result;
        }

        static public void Enqueue<T>(this List<T> list, T t)
        {
            list.Add(t);
        }
    }

    public static class GameObjectExtension
    {
        /// <summary>
        /// ����Ƿ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool HasComponent<T>(this GameObject obj)
        {
            T component = obj.GetComponent<T>();
            return component != null && component.ToString() != "null";
        }

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.AddComponent<T>();
            }
            return component;
        }

        public static UnityEngine.Object GetOrAddComponent(this GameObject obj, Type type)
        {
            UnityEngine.Object component = obj.GetComponent(type);
            if (component == null)
            {
                component = obj.AddComponent(type);
            }
            return component;
        }

        /// <summary>
        /// ���������ӽڵ�
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyAllChildren(this GameObject obj)
        {
            int childCount = obj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject.Destroy(obj.transform.GetChild(i).gameObject);
            }
        }
    }

    public static class Tools
    {

#if UNITY_ANDROID
        private static AndroidJavaObject _javaObj;
#endif

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void GetFreeDiskSpace(string productCfg);
#endif

#if UNITY_STANDALONE_WIN
        [DllImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceEx")]
        public static extern int GetDiskFreeSpaceEx(string lpDirectoryName, out long lpFreeBytesAvailable, out long lpTotalNumberOfBytes, out long lpTotalNumberOfFreeBytes);
#endif
        public static long GetFreeDiskSpace()
        {

#if UNITY_ANDROID
            if(_javaObj == null)
            {
                _javaObj = new AndroidJavaObject("com.unity3d.player.Tools");
            }
            return _javaObj.CallStatic<long>("GetFreeDiskSpace");
#endif

#if UNITY_IOS
            return GetFreeDiskSpace();
#endif

#if UNITY_STANDALONE_WIN
            long fb, ftb, tfb;
            string driveLetter = @"C:";
            if (GetDiskFreeSpaceEx(driveLetter, out fb, out ftb, out tfb) != 0)
            {
                // long total = Convert.ToInt64(fb / 1024 / 1024);
                // long enableUsetotal = Convert.ToInt64(ftb / 1024 / 1024);
                return Convert.ToInt64(tfb);
            }
            else
            {
                return -1;
            }
#endif
            return 0;
        }
    } 
}