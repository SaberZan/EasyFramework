using System.Threading;
namespace Easy
{

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using Newtonsoft.Json;
    using UnityEngine;

    /// <summary>
    /// 数据层
    /// </summary>
    [NormalInit]
    [OrderIndex((int) NormalInitOrderIndexEnum.SaveDataMgr)]
    public class SaveDataMgr : Singleton<SaveDataMgr>
    {
        private bool _inited = false;
        private Dictionary<string, Type> _allTypes;
        private Dictionary<string, SaveData> _allDatas;
        private Queue<Dictionary<string, SaveData>> _allCopyDatas;
        private SerializableDictionary<string, string> _stringPrefs;
        private object _saveLock;

        private string _savePath;


        private Stack<string> saveInfo = new Stack<string>();
        private bool saveTag = false;
        private int saveIndex = 0;

        private bool threadSave = false;
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override void Init(InitCompleteCallback complete)
        {
            _savePath = Application.persistentDataPath;
            _saveLock = new object();
            _allTypes = new Dictionary<string, Type>();
            _allDatas = new Dictionary<string, SaveData>();
            _allCopyDatas = new Queue<Dictionary<string, SaveData>>();
            //查找全部数据类
            List<Type> types = EasyFrameworkMain.Instance.GetTypes();
            foreach (var t in types)
            {
                if (typeof(SaveData).IsAssignableFrom(t) && t != typeof(SaveData))
                {
                    _allTypes.Add(t.Name, t);
                }
            }
            ReadAll();
            _inited = true;
            if(EasyTaskRunner.IsStartThreadTiming)
            {
                threadSave = true;
                EasyTaskRunner.Run(()=>WriteAllByChildThread()).Trigger();
            }
            complete.Invoke(true);
        }

        /// <summary>
        /// 重启之前
        /// </summary>
        public override void BeforeRestart()
        {
            saveInfo.Clear();
            saveTag = false;
            _inited = false;
            DeleteAll();
        }

        public override void RemoveInstance()
        {
            SaveAll(1000);
        }

        /// <summary>
        /// ReadAll
        /// </summary>
        /// <returns></returns>
        public void ReadAll()
        {
            SerializableDictionary<string, string> stringPrefs = FileMgr.Instance.GetTargetClassObject<SerializableDictionary<string, string>>(_savePath + "/prefs.bin") ?? new SerializableDictionary<string, string>();
            foreach (var kv in stringPrefs)
            {
                string typeName = GetTypeNameFromKeyName(kv.Key);
                if(_allTypes.ContainsKey(typeName))
                {
                    Type t = _allTypes[typeName];
                    SaveData data = ReadSaveData(t, kv.Value);
                    if (data == null)
                    {
                        data = (SaveData) t.GetConstructor(Type.EmptyTypes)?.Invoke(null);
                    }
                    _allDatas.Add(kv.Key, data);
                }
            }

            _stringPrefs = stringPrefs;
        }

    
        /// <summary>
        /// 输出数据
        /// </summary>
        /// <returns></returns>
        public string ToDataString()
        {
            JSONObject allDatasJson = new JSONObject();
            foreach (var kv in _stringPrefs)
            {
                allDatasJson.AddField(kv.Key, kv.Value);
            }
            return allDatasJson.ToString();
        }


        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns></returns>
        public bool LoadDataString(string dataString)
        {
            try
            {
                JSONObject allDatasJson = new JSONObject(dataString);
                foreach (string key in allDatasJson.keys)
                {
                    allDatasJson.GetField(out string dataJson, key, "");
                    byte[] byStr = Convert.FromBase64String(dataJson);
                    string str = Encoding.UTF8.GetString(byStr);
                    WriteUserDataStorage(key, str, _stringPrefs);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(bool autoCreate = true) where T : SaveData, new()
        {
            if (!_inited) return null;
            string key = typeof(T).Name;
            if (typeof(T) != typeof(KeyData) && Get<KeyData>().Keys.ContainsKey(key))
            {
                key = GetKeyName(key, Get<KeyData>().Keys[key]);
            }
            if (_allDatas.ContainsKey(key))
            {
                return (T) _allDatas[key];
            }
            else
            {
                if(autoCreate)
                {
                    T t = new T();
                    Set<T>(t);
                    return t;
                }
                return null;
            }
        }

        /// <summary>
        /// 通过key获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public T GetByKey<T>(string keyName, bool autoCreate = true) where T : SaveData, new()
        {
            string typeName = typeof(T).Name;
            if (_allDatas.ContainsKey(typeName))
            {
                keyName = GetKeyName(typeName, keyName);
                return (T) _allDatas[keyName];
            }
            else
            {
                if(autoCreate)
                {
                    T t = new T();
                    SetByKey<T>(keyName, t);
                    return t;
                }
                return null;
            }
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userData"></param>
        public void Set<T>(T userData) where T : SaveData, new()
        {
            if (!_inited) return;
            string key = typeof(T).Name;
            if (_allDatas.ContainsKey(key))
            {
                _allDatas[key] = userData;
            }
            else
            {
                _allDatas.Add(key, userData);
            }
        }

        /// <summary>
        /// 设置分key的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <param name="userData"></param>
        public void SetByKey<T>(string keyName, T userData) where T : SaveData, new()
        {
            if (!_inited) return;
            string key = typeof(T).Name;
            keyName = GetKeyName(key, keyName);
            if (_allDatas.ContainsKey(keyName))
            {
                _allDatas[keyName] = userData;
            }
            else
            {
                _allDatas.Add(keyName, userData);
            }
        }

        /// <summary>
        /// 设置当前key, get时会默认对当前key
        /// </summary>
        /// <param name="keyName"></param>
        /// <typeparam name="T"></typeparam>
        public void SetCurrentKey<T>(string keyName) where T : SaveData, new()
        {
            if (!_inited) return;
            string key = typeof(T).Name;
            keyName = GetKeyName(key, keyName);

            if(!_allDatas.ContainsKey(keyName))
            {
                throw new Exception("No key data");
            }
            
            if (!Get<KeyData>().Keys.ContainsKey(key))
            {
                Get<KeyData>().Keys.Add(key, keyName);
            }
            else
            {
                Get<KeyData>().Keys[key] = keyName;
            }
        }

        /// <summary>
        /// 设置当前key的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <param name="userData"></param>
        public void SetByCurrentKey<T>(string keyName, T userData) where T : SaveData, new()
        {
            if (!_inited) return;
            SetByKey<T>(keyName, userData);
            SetCurrentKey<T>(keyName);
        }

        /// <summary>
        /// 删除有分key的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <param name="nowKeyName"></param>
        public void DeleteByKey<T>(string keyName, string nowKeyName = "") where T : SaveData, new()
        {
            if (!_inited) return;
            string key = typeof(T).Name;
            _allDatas.Remove(GetKeyName(key, keyName));
            SetCurrentKey<T>(nowKeyName);
        }

        /// <summary>
        /// 清空key对应的数据
        /// </summary>
        /// <returns></returns>
        public void DeleteData<T>() where T : SaveData, new()
        {
            string typeName = typeof(T).Name;
            foreach (var key in _allDatas.Keys)
            {
                if(typeName == GetTypeNameFromKeyName(key))
                {
                    _allDatas.Remove(key);
                }
            }
            if(Get<KeyData>().Keys.ContainsKey(typeName))
            {
                Get<KeyData>().Keys.Remove(typeName);
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        /// <returns></returns>
        public void DeleteAll()
        {
            _allDatas.Clear();
        }

        /// <summary>
        /// 获取存储key
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public string GetKeyName(params string[] keys)
        {
            if(keys.Length == 0)
            {
                return string.Empty;
            }
            else if(keys.Length == 1)
            {
                return keys[0];
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (keys[0] != null)
                {
                    stringBuilder.Append(keys[0].ToString());
                }
                for (int i = 1; i < keys.Length; i++)
                {
                    stringBuilder.Append("_");
                    if (keys[i] != null)
                    {
                        stringBuilder.Append(keys[i].ToString());
                    }
                }
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// 获取存储的类型
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetTypeNameFromKeyName(string key)
        {
            int index = key.IndexOf('_');
            if(index == -1)
            {
                return key;
            }
            else
            {
                return key.Substring(0, index);
            }
        }
        
        /// <summary>
        /// 获取用户保存的数据
        /// </summary>
        /// <typeparam name="T">返回的对象类型</typeparam>
        /// <param name="key">键值</param>
        /// <returns>需要的对象</returns>
        private T ReadSaveData<T>(string contentStr) where T : SaveData
        {
            if (string.IsNullOrEmpty(contentStr))
                return null;
            contentStr = XOREncryption.DecryptData(contentStr, XOREncryption.DEFAULT_ENCRYPT_KEY);
            T t = JsonConvert.DeserializeObject<T>(contentStr);
            return t;
        }

        /// <summary>
        /// 获取用户保存的数据
        /// </summary>
        /// <typeparam name="T">返回的对象类型</typeparam>
        /// <param name="key">键值</param>
        /// <param name="type"></param>
        /// <returns>需要的对象</returns>
        private SaveData ReadSaveData(Type type, string contentStr)
        {
            if (string.IsNullOrEmpty(contentStr))
                return null;
            contentStr = XOREncryption.DecryptData(contentStr, XOREncryption.DEFAULT_ENCRYPT_KEY);
            SaveData t = (SaveData)JsonConvert.DeserializeObject(contentStr, type);
            return t;
        }
        
        /// <summary>
        /// 获得用户数据
        /// </summary>
        private string ReadUserDataStorage(string key, string defaultValue, SerializableDictionary<string, string> stringPrefs)
        {
            if (stringPrefs.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 保存用户数据
        /// </summary>
        private void WriteUserDataStorage(string key, string contentStr, SerializableDictionary<string, string> stringPrefs)
        {
            if (!stringPrefs.ContainsKey(key))
            {
                if(!string.IsNullOrEmpty(contentStr))
                {
                    stringPrefs.Add(key,contentStr);
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(contentStr))
                {
                    stringPrefs[key] = contentStr;
                }
                else
                {
                    stringPrefs.Remove(key);
                }
            }    
        }

        /// <summary>
        /// 保存需要保存的对象数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="saveData"></param>
        private void SaveSaveData(string key, SaveData saveData, SerializableDictionary<string, string> stringPrefs)
        {
            string contentStr = JsonConvert.SerializeObject(saveData);
            contentStr = XOREncryption.EncryptData(contentStr, XOREncryption.DEFAULT_ENCRYPT_KEY);
            WriteUserDataStorage(key, contentStr, stringPrefs);
        }

        /// <summary>
        /// 拷贝数据
        /// </summary>
        private void CopyData()
        {
            var allCopyDatas = ObjectPoolMgr.Instance.GetObject<Dictionary<string, SaveData>>();
            foreach(var key in allCopyDatas.Keys)
            {
                if(!_allDatas.ContainsKey(key))
                {
                    allCopyDatas.Remove(key);
                }
            }

            foreach (var kv in _allDatas)
            {
                if(!allCopyDatas.ContainsKey(kv.Key))
                {
                    Type t = kv.Value.GetType();
                    SaveData copyData = (SaveData) t.GetConstructor(Type.EmptyTypes)?.Invoke(null);
                    allCopyDatas.Add(kv.Key, copyData);
                }
                allCopyDatas[kv.Key].CopyValue(kv.Value);
            }
            _allCopyDatas.Enqueue(allCopyDatas);
        }

        /// <summary>
        /// 写数据
        /// </summary>
        private void WriteData()
        {
            if(_allCopyDatas.Count <= 0)
            {
                return;
            }
            var allCopyDatas = _allCopyDatas.Dequeue();
            KeyData keyData = (KeyData)allCopyDatas["KeyData"];
            foreach (var kv in allCopyDatas)
            {
                string dataKey = kv.Value.GetType().Name;
                if(keyData.Keys.ContainsKey(dataKey))
                {
                    if(keyData.Keys[dataKey] == kv.Key)
                    {
                        SaveSaveData(kv.Key, kv.Value, _stringPrefs);
                    }
                }
                else
                {
                    SaveSaveData(kv.Key, kv.Value, _stringPrefs);
                }
            }

            foreach (var key in _stringPrefs.Keys)
            {
                if(!allCopyDatas.ContainsKey(key))
                {
                    _stringPrefs.Remove(key);
                }
            }
            FileMgr.Instance.SetTargetClassObjectToFile(_savePath + "/prefs.bin", _stringPrefs);
            ObjectPoolMgr.Instance.PutObject(allCopyDatas);
        }

        /// <summary>
        /// SaveAll
        /// </summary>
        /// <returns></returns>
        public void SaveAll(int millisecondsTimeout = 100)
        {
            bool result = false;
            try
            {
                result = Monitor.TryEnter(_saveLock, millisecondsTimeout);
                if(result)
                {
                    CopyData();
                    while(_allCopyDatas.Count > 0)
                    {
                        WriteData();
                    }
                }
            }
            finally
            {
                if(result)
                {
                    Monitor.Exit(_saveLock);
                }
            }            
        }

        /// <summary>
        /// 主线程调用
        /// </summary>
        private bool CopyDataByMainThread()
        {
            bool result = false;
            //快速拷贝所有数据
            try
            {
                result = Monitor.TryEnter(_saveLock);
                if(result)
                {
                    CopyData();
                }
            }
            finally
            {
                if(result)
                {
                    Monitor.Exit(_saveLock);
                }
            }
            return result;
        }

        /// <summary>
        /// SaveAll
        /// </summary>
        /// <returns></returns>
        private void WriteAllByChildThread()
        {
            while(true)
            {
                bool result = false;
                try
                {
                    result = Monitor.TryEnter(_saveLock);
                    if(result)
                    {
                        WriteData();
                    }
                }
                finally
                {
                    if(result)
                    {
                        Monitor.Exit(_saveLock);
                    }
                }

                if(_allCopyDatas.Count == 0)
                {  
                    Thread.Sleep(100);  
                }
                else
                {
                    Thread.Sleep(10);
                }        
            }
        }

        /// <summary>
        /// 开始保存数据
        /// </summary>
        public void BeginSave(string key)
        {
            if(saveInfo.Count == 0)
            {
                saveTag = true;
            }
            saveInfo.Push(key);   
        }

        /// <summary>
        /// 结束保存数据
        /// </summary>
        public void EndSave(string key)
        {
            saveInfo.Pop();
            if(saveTag && saveInfo.Count == 0)
            {
                CopyDataByMainThread();
                if(!threadSave)
                {
                    WriteData();
                }
            }
        }
    }

}