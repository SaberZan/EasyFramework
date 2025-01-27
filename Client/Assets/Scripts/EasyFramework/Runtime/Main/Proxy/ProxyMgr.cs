namespace Easy
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    
    /// <summary>
    /// 业务数据代理接口层
    /// </summary>
    [NormalInit]
    [OrderIndex((int) NormalInitOrderIndexEnum.ProxyMgr)]
    public class ProxyMgr : Singleton<ProxyMgr>
    {
        private bool _inited = false;

        private Dictionary<string, Proxy> _allProxy = new Dictionary<string, Proxy>();

        public override void BeforeRestart()
        {

        }

        public override void Init(InitCompleteCallback complete)
        {
            if (!_inited)
            {
                //查找全部数据类
                List<Type> types = EasyFrameworkMain.Instance.GetTypes();
                var ProxyType = typeof(Proxy);
                foreach (var t in types)
                {
                    if(t.IsAbstract)
                    {
                        continue;
                    }
                    if (ProxyType.IsAssignableFrom(t))
                    {
                        string key = t.FullName??t.Name;
                        Proxy obj = (Proxy)t.Assembly.CreateInstance(key);
                        _allProxy.Add(key, obj);
                    }
                }

                _inited = true;
            }

            complete.Invoke(true);
        }

        /// <summary>
        /// 获取代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : Proxy
        {
            if (!_inited) return null;
            string key = typeof(T).FullName;
            if (_allProxy.ContainsKey(key))
            {
                return (T) _allProxy[key];
            }
            else
            {
                return null;
            }
        }
    }

}