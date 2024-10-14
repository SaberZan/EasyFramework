namespace Easy
{

    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// 协同管理器
    /// </summary>
    [NormalInit]
    [OrderIndex((int) NormalInitOrderIndexEnum.CoroutineMgr)]
    public class CoroutineMgr : Singleton<CoroutineMgr>
    {
        private GameObject _coroutineHost;

        /// <summary>
        /// 挂哉协同方法的对象
        /// </summary>
        private MonoBehaviour _behaviour;

        public override void Init(InitCompleteCallback complete)
        {
            _coroutineHost = new GameObject("CorountineHost");
            GameObject.DontDestroyOnLoad(_coroutineHost);
            _behaviour = _coroutineHost.AddComponent<CoroutineBehaviour>();

            complete.Invoke(true);
        }

        public override void BeforeRestart()
        {
            StopAllCoroutine();
            GameObject.Destroy(_coroutineHost);
            //Init((bOk) => Debug.Log("CoroutineManager restart"));
        }

        #region 通过方法名称开启和关闭协同

        [Obsolete("Don't use the function! Instend of using \"StartCoroutine(IEnumerator coroutine)\"")]
        public bool StartCoroutine(string funcname, object value = null)
        {
            if (_behaviour == null) return false;
            if (value == null)
                _behaviour.StartCoroutine(funcname);
            else
                _behaviour.StartCoroutine(funcname, value);
            return true;
        }

        public bool StopCoroutine(string funcname)
        {
            if (_behaviour == null) return false;
            _behaviour.StopCoroutine(funcname);
            return true;
        }

        #endregion

        #region 通过枚举器开启和关闭协同

        public Coroutine StartCoroutine(IEnumerator coroutine)
        {
            if (_behaviour == null) return null;

            return _behaviour.StartCoroutine(coroutine);
            //return true;
        }

        public bool StopCoroutine(IEnumerator coroutine)
        {
            if (_behaviour == null) return false;
            _behaviour.StopCoroutine(coroutine);
            return true;
        }

        public bool StopCoroutine(Coroutine coroutine)
        {
            if (_behaviour == null) return false;
            _behaviour.StopCoroutine(coroutine);
            return true;
        }

        public void StopAllCoroutine()
        {
            if (_behaviour == null) return;
            _behaviour.StopAllCoroutines();
        }

        #endregion
    }

}