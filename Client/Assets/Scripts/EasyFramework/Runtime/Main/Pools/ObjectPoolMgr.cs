using System.Collections.Generic;

namespace Easy
{
    /// <summary>
    /// 缓存值对象
    /// </summary>
    public class ObjectPoolMgr : Singleton<ObjectPoolMgr>
    {
        private readonly Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();

        public override void Init(InitCompleteCallback complete)
        {
            complete.Invoke(true);
        }

        public override void BeforeRestart()
        {
            lock(_pools)
            {
                _pools.Clear();
            }
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T GetObject<T>() where T : class, IPoolObject, new()
        {
            lock(_pools)
            {
                string name = typeof(T).FullName;
                if (!_pools.ContainsKey(name))
                {
                    _pools.Add(name,new ObjectPool());
                }
                return _pools[name].Get<T>();
            }
        }
        
        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj"></param>
        public void PutObject(IPoolObject obj)
        {
            lock(_pools)
            {
                string name = obj.GetType().FullName;
                if (!_pools.ContainsKey(name))
                {
                    _pools.Add(name,new ObjectPool());
                }
                obj.Reset();
                _pools[name].Put(obj);
            }
        }

        /// <summary>
        /// 设置对象池最大值
        /// </summary>
        /// <param name="maxNum"></param>
        /// <typeparam name="T"></typeparam>
        public void SetPoolMaxNum<T>(int maxNum)
        {
            lock(_pools)
            {
                string name = typeof(T).FullName;
                if (_pools.ContainsKey(name))
                {
                    _pools[name].SetMaxNum(maxNum);
                }
            }
        }
    }
}