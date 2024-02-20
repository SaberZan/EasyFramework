using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Easy
{
    /// <summary>
    /// 埋点单例类
    /// </summary>    
    [PreInit]
    [OrderIndex((int)PreInitOrderIndexEnum.AnalysisMgr)]
    public class AnalysisMgr : Singleton<AnalysisMgr>
    {
        private HashSet<IAnalysis> _allAnalysis = new HashSet<IAnalysis>();

        //单例异步加载计数
        private int _waitToInit;

        private string _adid = "";

        public bool _initsuccess = false;

        //初始化
        public override void Init(InitCompleteCallback complete)
        {
            if (_initsuccess)
            {
                complete?.Invoke(true);
                return;
            }

            //D.Log("Analysis : *************************************************** 1");
            //查找全部埋点类
            var types = GetType().Assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.GetInterfaces().Contains(typeof(IAnalysis)))
                {
                    IAnalysis analysis = (IAnalysis) t.GetConstructor(Type.EmptyTypes).Invoke(null);
                    _allAnalysis.Add(analysis);
                }
            }

            //D.Log("Analysis : *************************************************** 2");
            //加载，包含异步处理

            if (_allAnalysis.Count <= 0)
            {
                _initsuccess = true;
                complete.Invoke(true);
                return;
            }
            
            _waitToInit = _allAnalysis.Count;
            foreach (IAnalysis analysis in _allAnalysis)
            {
                analysis.Init((inited) =>
                {
                    if (inited)
                    {
                        _waitToInit--;
                        if (_waitToInit == 0)
                        {
                            _initsuccess = true;
#if UNITY_EDITOR
                            complete.Invoke(true);
#else
                            //异步获取广告ID
                            Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string error) =>
                            {
                                EasyLogger.Log("EasyFrameWork","Analysis - AdvertisingId " + advertisingId + " " + trackingEnabled + " " + error);
                                _adid = advertisingId;
                                foreach (IAnalysis item in _allAnalysis)
                                {
                                    item.SetAdvertiseId(advertisingId);
                                }
                            });                        
                            complete.Invoke(true);
#endif
                        }
                    }
                    else
                    {
                        complete.Invoke(false);
                        _initsuccess = true;
                    }
                });
            }
        }

        public override void BeforeRestart()
        {

        }

        //广告标识
        public string GetAdvertiseID()
        {
            return _adid;
        }

        //用户唯一标识
        public string GetUniqueId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        //发送消息
        public void SendEvent(string key, JSONObject param = null)
        {
#if DEBUG
            //D.Log("<color=yellow>埋点记录DEBUG</color> " + key + " - " + (param == null ? "参数null" : param.ToString()));
#else
            //D.Log("<color=yellow>埋点记录RELEASE</color> " + key + " - " + (param == null ? "参数null" : param.ToString()));
            foreach (IAnalysis item in _allAnalysis)
            {
                try
                {
                    item.SendEvent(key, param);
                }
                catch (Exception err)
                {
                    //D.LogError("埋点错误:" + item.GetType().FullName + " Error:" + err.Message);
                }
            }
#endif
        }
    }
    
}