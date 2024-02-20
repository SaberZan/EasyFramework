namespace Easy
{

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// 数据层
    /// </summary>
    [NormalInit]
    [OrderIndex((int) NormalInitOrderIndexEnum.CacheDataMgr)]
    public class CacheDataMgr : Singleton<CacheDataMgr>
    {
        private bool _inited = false;
        private Dictionary<string, Type> _allTypes;
        private Dictionary<string, CacheData> _allDatas;
        private List<Type> _listTypes;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override void Init(InitCompleteCallback complete)
        {
            _allTypes = new Dictionary<string, Type>();
            _listTypes = new List<Type>();
            //查找全部数据类
            List<Type> types = EasyFrameworkMain.Instance.GetTypes();
            foreach (var t in types)
            {
                if (typeof(CacheData).IsAssignableFrom(t))
                {
                    _listTypes.Add(t);
                }
            }
            OrderIndexAttribute.Sort<Type>(_listTypes);

            foreach (var t in _listTypes)
            {
                string key = t.Name;
                _allTypes.Add(key, t);
            }

            ReadAll();
            _inited = true;
            complete.Invoke(true);
        }

        /// <summary>
        /// 重启之前
        /// </summary>
        public override void BeforeRestart()
        {
            // SaveAll();
        }

        /// <summary>
        /// ReadAll
        /// </summary>
        /// <returns></returns>
        public void ReadAll()
        {
            _allDatas = new Dictionary<string, CacheData>();
            var types = _allTypes.Values;
            foreach (var t in types)
            {
                if (typeof(CacheData).IsAssignableFrom(t) && t != typeof(CacheData))
                {
                    string key = t.Name;
                    CacheData data = (CacheData) t.GetConstructor(Type.EmptyTypes)?.Invoke(null);
                    data.Init();
                    _allDatas.Add(key, data);
                }
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : CacheData, new()
        {
            if (!_inited) return null;
            string key = typeof(T).Name;
            if (_allDatas.ContainsKey(key))
            {
                return (T) _allDatas[key];
            }
            else
            {
                return null;
            }
        }
    }

}