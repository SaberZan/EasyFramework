namespace Easy
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 对象池
    /// </summary>
    public class GameObjectPool
    {
        private ISingleUnityAssetHandle<GameObject> _sourceHandle;
        private GameObject _rootNode;
        private int _max = -1;
        private bool _hide = false;
        private static Vector3 _HIDE_POS = Vector3.one * (999999f);

        /// <summary>
        /// 构造函数
        /// 自动消息:
        ///     Used - 获取时触发
        ///     Unused - 放回时触发
        /// </summary>
        public GameObjectPool(ISingleUnityAssetHandle<GameObject> handle)
        {
            _rootNode = new GameObject(handle.GetResult().name + "Pool");
            _rootNode.transform.position = GameObjectPool._HIDE_POS;
            Object.DontDestroyOnLoad(_rootNode);
            _sourceHandle = handle;
        }

        /// <summary>
        /// 构造函数
        /// 自动消息:
        ///     Used - 获取时触发
        ///     Unused - 放回时触发
        /// </summary>
        public GameObjectPool(string prefabPath)
        {
            var handle = Easy.AssetsMgr.Instance.LoadAsset<GameObject>(prefabPath);
            _rootNode = new GameObject(handle.GetResult().name + "Pool");
            _rootNode.transform.position = GameObjectPool._HIDE_POS;
            Object.DontDestroyOnLoad(_rootNode);
            _sourceHandle = handle;
        }
        
        /// <summary>
        /// 构造函数
        /// 自动消息:
        ///     Used - 获取时触发
        ///     Unused - 放回时触发
        /// </summary>
        /// <param prefabPath="gameObject">源对象路径</param>
        public GameObjectPool(GameObject gameObject)
        {
            _rootNode = new GameObject(gameObject.name + "Pool");
            _rootNode.transform.position = GameObjectPool._HIDE_POS;
            Object.DontDestroyOnLoad(_rootNode);
        }

        /// <summary>
        /// 从缓存中拿
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        public GameObject Get(GameObject sourceObject = null)
        {
            GameObject result = GetFromCache(sourceObject);
            return result;
        }

        /// <summary>
        /// 从缓存中拿
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        private GameObject GetFromCache(GameObject sourceObject)
        {
            GameObject result;
            if (_rootNode.transform.childCount > 0)
            {
                result = _rootNode.transform.GetChild(0).gameObject;
                if (_hide)
                    result.SetActive(true);
                result.transform.parent = null;
            }
            else
            {
                result = sourceObject ? GameObject.Instantiate(sourceObject) : _sourceHandle.Instantiate();
            }

            result.SendMessage("Used", SendMessageOptions.DontRequireReceiver);
            return result;
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="gameObject"></param>
        public void Put(GameObject gameObject)
        {
            PutToCache(gameObject);
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="gameObject"></param>
        private void PutToCache(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            if (_rootNode == null)
            {
                Object.Destroy(gameObject);
                return;
            }

            if (_max == -1 || _rootNode.transform.childCount < _max )
            {
                gameObject.transform.position = _rootNode.transform.position;
                gameObject.transform.SetParent(_rootNode.transform);
                gameObject.SendMessage("Unused", SendMessageOptions.DontRequireReceiver);
                if (_hide)
                    gameObject.SetActive(false);
            }
            else
            {
                Object.Destroy(gameObject);
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            if (_sourceHandle != null)
            {
                for (int i = _rootNode.transform.childCount - 1; i >= 0; --i)
                {
                    _sourceHandle.ReleaseInstance(_rootNode.transform.GetChild(i).gameObject);
                }
                Easy.AssetsMgr.Instance.Release(_sourceHandle);
            }
            else
            {
                for (int i = _rootNode.transform.childCount - 1; i >= 0; --i)
                {
                    GameObject.Destroy(_rootNode.transform.GetChild(i));
                }
            }
            Object.Destroy(_rootNode);
        }

        /// <summary>
        /// 设置缓存最大值
        /// </summary>
        /// <param name="maxNum"></param>
        public void SetMaxNum(int maxNum)
        {
            _max = maxNum;
        }

        /// <summary>
        /// 设置缓存的时候是否隐藏
        /// </summary>
        /// <param name="value"></param>
        public void SetHide(bool value)
        {
            _hide = value;
        }
    }

}