namespace Easy
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GameObjectPoolMgr : Singleton<GameObjectPoolMgr>
    {

        private Dictionary<string, GameObjectPool> _pools = new Dictionary<string, GameObjectPool>();
        
        public override void BeforeRestart()
        {

        }

        public override void Init(InitCompleteCallback complete)
        {
            complete.Invoke(true);
        }
        
        public override void RemoveInstance()
        {
            base.RemoveInstance();
            foreach (KeyValuePair<string, GameObjectPool> keyValuePair in _pools)
            {
                keyValuePair.Value.Release();
            }

            _pools.Clear();
        }

        /// <summary>
        /// 对象池是否存在
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <returns></returns>
        public bool IsPoolExsit(string prefabPath)
        {
            return _pools.ContainsKey(prefabPath);
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <returns></returns>
        public GameObjectPool CreatePool(string prefabPath)
        {
            if (!_pools.ContainsKey(prefabPath))
                _pools.Add(prefabPath, new GameObjectPool(prefabPath));
            return _pools[prefabPath];
        }
        
        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="key"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public GameObjectPool CreatePool(string key, GameObject gameObject)
        {
            if (!_pools.ContainsKey(key))
                _pools.Add(key, new GameObjectPool(gameObject));
            return _pools[key];
        }

        /// <summary>
        /// 清除对象池
        /// </summary>
        /// <param name="prefabPath"></param>
        public void ClearPool(string prefabPath)
        {
            if (_pools.ContainsKey(prefabPath))
            {
                _pools[prefabPath].Release();
                _pools.Remove(prefabPath);
            }
        }

        /// <summary>
        /// 获得对象
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <returns></returns>
        public GameObject GetGameObject(string prefabPath)
        {
            if (!_pools.ContainsKey(prefabPath))
                _pools.Add(prefabPath, new GameObjectPool(prefabPath));
            return _pools[prefabPath].Get();
        }
        
        /// <summary>
        /// 获得对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public GameObject GetGameObject(string key, GameObject gameObject)
        {
            if (!_pools.ContainsKey(key))
                _pools.Add(key, new GameObjectPool(gameObject));
            return _pools[key].Get(gameObject);
        }

        /// <summary>
        /// 缓存对象
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <param name="gameObject"></param>
        public void PutGameObject(string prefabPath, GameObject gameObject)
        {
            if (!_pools.ContainsKey(prefabPath))
                _pools.Add(prefabPath, new GameObjectPool(prefabPath));
            _pools[prefabPath].Put(gameObject);
        }

        /// <summary>
        /// 设置最大缓存值
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <param name="maxNum"></param>
        public void SetPoolMaxNum(string prefabPath, int maxNum)
        {
            if (!_pools.ContainsKey(prefabPath))
                _pools.Add(prefabPath, new GameObjectPool(prefabPath));
            _pools[prefabPath].SetMaxNum(maxNum);
        }

        /// <summary>
        /// 设置缓存是否隐藏
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <param name="value"></param>
        public void SetPoolHide(string prefabPath, bool value)
        {
            if (!_pools.ContainsKey(prefabPath))
                _pools.Add(prefabPath, new GameObjectPool(prefabPath));
            _pools[prefabPath].SetHide(value);
        }
    }

}
