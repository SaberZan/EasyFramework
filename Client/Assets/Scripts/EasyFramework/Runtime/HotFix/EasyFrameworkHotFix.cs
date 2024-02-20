namespace Easy
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// 框架启动入口
    /// </summary>
    public class EasyFrameworkHotFix
    {
        /// <summary>
        /// 初始化进度
        /// </summary>
        public float initProgress = 0;

        /// <summary>
        /// 正在初始化的模块
        /// </summary>
        public List<string> initializingSingles;

        /// <summary>
        /// 成功回调
        /// </summary>
        private InitCompleteCallback _singletonBatchInitCallBack;

        /// <summary>
        /// 正常加载单例容器
        /// </summary>
        private Dictionary<string, ISingleton> _initModules;
        
        /// <summary>
        /// SingletonUpdate
        /// </summary>
        private SingletonUpdateMonoBehaviour _singletonUpdate;
        
        /// <summary>
        /// 单例
        /// </summary>
        private static EasyFrameworkHotFix _instance;

        /// <summary>
        /// 线程锁
        /// </summary>
        private static object _lockObj = new object();

        /// <summary>
        /// 获取单例
        /// </summary>
        public static EasyFrameworkHotFix Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        if (_instance == null)
                            _instance = new EasyFrameworkHotFix();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// 私有构造
        /// </summary>
        private EasyFrameworkHotFix()
        {
            //遍历所有类
            List<Type> types = EasyFrameworkMain.Instance.GetTypes();
            
            //容器初始化
            _initModules = new Dictionary<string, ISingleton>();
            
            foreach (var t in types)
            {
                if (!t.IsAbstract && !t.IsInterface) //接口抽象类不处理
                {
                    if (t.GetInterface("ISingleton") != null && t.BaseType != null)
                    {
                        PropertyInfo property = t.BaseType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
                        if (property != null)
                        {
                            ISingleton singleton = (ISingleton) property.GetValue(null);
                            //优先
                            if (t.IsDefined(typeof(NormalInitAttribute), false))
                            {
                                _initModules.Add(t.Name, singleton);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="onInitCompleted">成功回调</param>
        public void Init(InitCompleteCallback onInitCompleted)
        {
            if (IsInited())
            {
                EasyLogger.LogWarning("EasyFrameWork", "Framework已初始化.");
                onInitCompleted.Invoke(true);
                return;
            }

            _singletonUpdate = UnityEngine.Object.FindObjectOfType<SingletonUpdateMonoBehaviour>();
            if (_singletonUpdate == null)
            {
                GameObject singletonUpdateNode = new GameObject("SingletonUpdate");
                _singletonUpdate = singletonUpdateNode.AddComponent<SingletonUpdateMonoBehaviour>();
                UnityEngine.Object.DontDestroyOnLoad(singletonUpdateNode);
            }
            
            //优先单例
            InitializeSingleton("优先加载", onInitCompleted);
        }

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        /// <returns></returns>
        public bool IsInited()
        {
            return Mathf.Approximately(initProgress,1);
        }

        /// <summary>
        /// 卸载
        /// </summary>
        public void Dispose()
        {
            //重置单例
            ResetSingleton();

            _singletonUpdate = null;
            //清理Framework资源
            initProgress = 0;
            _instance = null;
            _initModules.Clear();
            //GC
            System.GC.Collect();
        }

        /// <summary>
        /// 初始化所有单例
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="complete"></param>
        private void InitializeSingleton(string tag, InitCompleteCallback complete)
        {
            EasyLogger.Log("EasyFrameWork", tag + ":单例初始化开始<color=#32CFE7>" + string.Join(",", _initModules.Keys.ToArray()) + "</color>");
            if (_initModules.Values.Count == 0)
            {
                complete.Invoke(false);
            }
            else
            {

                initializingSingles = new List<string>();
                Dictionary<int, List<ISingleton>> dic = OrderIndexAttribute.GetBatchListByInterval<ISingleton>(_initModules.Values.ToList<ISingleton>());
                List<int> keys = dic.Keys.ToList();
                keys.Sort();
                InitBatch(dic, keys, 0, complete);
            }
        }

        /// <summary>
        /// 批量初始化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="callback"></param>
        private void InitBatch(Dictionary<int, List<ISingleton>> dic, List<int> keys, int keyIndex, InitCompleteCallback callback)
        {
            List<ISingleton> list = dic[keys[keyIndex]];
            int index = 0;
            for (int i = 0; i < list.Count; ++i)
            {
                var singleTon = list[i];
                initializingSingles.Add(singleTon.GetType().Name);
                EasyLogger.Log("EasyFrameWork", "-HotFix-initializingSingle--" + string.Join(",", initializingSingles));
                singleTon.Init((result) =>
                {
                    if (result)
                    {
                        ++index;
                        initializingSingles.Remove(singleTon.GetType().Name);
                        if (singleTon.GetType().IsDefined(typeof(UpdateAttribute), false))
                        {
                            _singletonUpdate.updateCallBack += singleTon.Update;
                        }
                        if (index == list.Count)
                        {
                            ++keyIndex;
                            _singletonBatchInitCallBack?.Invoke(true);
                            initProgress = keyIndex * 1.0f / dic.Count;
                            if (keyIndex < keys.Count)
                            {
                                InitBatch(dic, keys, keyIndex, callback);
                            }
                            else
                            {
                                callback(true);
                            }
                        }
                    }
                    else
                    {
                        callback(false);
                    }
                });
            }
        }


        /// <summary>
        /// 添加单例模块初始化完成回调
        /// </summary>
        /// <param name="callback"></param>
        public void AddSingletonBatchInitCallback(InitCompleteCallback callback)
        {
            _singletonBatchInitCallBack += callback;
        }

        /// <summary>
        /// 删除单例模块初始化回调
        /// </summary>
        /// <param name="callback"></param>
        public void SubSingletonBatchInitCallback(InitCompleteCallback callback)
        {
            _singletonBatchInitCallBack -= callback;
        }


        /// <summary>
        /// 重置单例
        /// </summary>
        private void ResetSingleton()
        {
            //延迟加载的单例
            foreach (var singleton in _initModules)
            {
                singleton.Value.BeforeRestart();
                singleton.Value.RemoveInstance();
            }
        }
    }
}
