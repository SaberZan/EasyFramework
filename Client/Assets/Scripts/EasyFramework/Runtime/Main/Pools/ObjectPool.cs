namespace Easy
{

    using System;
    using System.Collections.Concurrent;
    
    /// <summary>
    /// 接口：池对象
    /// </summary>
    public interface IPoolObject 
    {
        void Reset();
    }
    
    /// <summary>
    /// 对象池类
    /// 需要对象池的地方，构建一个对象池
    /// 加入工厂方法、回收方法、对象个数
    /// </summary>
    public class ObjectPool
    {
        public int _max = -1;
        
        /// <summary>
        /// 缓存栈（所有对象保存的容器）
        /// </summary>
        private ConcurrentStack<IPoolObject> _cacheStack = new ConcurrentStack<IPoolObject>();
        
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <returns>对象</returns>
        public T Get<T>() where T : class, new()
        {
            if (_cacheStack.Count == 0)
            {
                return new T();
            }
            else
            {
                if (_cacheStack.TryPop(out IPoolObject t))
                    return (T)t;
            }
            return default(T);
        }

        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="obj">类型对象</param>
        /// <returns></returns>
        public void Put(IPoolObject obj)
        {
            if (_max == -1 || _cacheStack.Count < _max)
            {
                obj.Reset();
                _cacheStack.Push(obj);
            }
            else
            {
                obj = null;
            }
        }
        
        /// <summary>
        /// 设置缓存最大值
        /// </summary>
        /// <param name="maxNum"></param>
        public void SetMaxNum(int maxNum)
        {
            _max = maxNum;
        }
    }
}