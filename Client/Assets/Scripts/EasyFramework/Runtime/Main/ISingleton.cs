namespace Easy
{

    using System;

//加载回调
    public delegate void InitCompleteCallback(bool inited);

    /// <summary>
    /// 单例接口
    /// </summary>
    public interface ISingleton
    {
        void Init(InitCompleteCallback complete);
        void RemoveInstance();
        void BeforeRestart();
        void Update(float deltaTime);
    }

//优先加载
    public class PreInitAttribute : Attribute
    {
    }
    
//优先加载
    public class NormalInitAttribute : Attribute
    {
    }
    
//Update
    public class UpdateAttribute : Attribute
    {
    }

    /// <summary>
    /// 单例基类
    /// </summary>
    /// <typeparam name="T">派生类对象</typeparam>
    public abstract class Singleton<T> : ISingleton where T : class, new()
    {
        private static T _instance;
        private static object _lockObj = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        if (_instance == null)
                            _instance = new T();
                    }
                }

                return _instance;
            }
        }

        //初始化
        public abstract void Init(InitCompleteCallback complete);
        
        //初始化前准备
        public abstract void BeforeRestart();

        //重置
        public virtual void RemoveInstance()
        {
            _instance = null;
        }

        //Update
        public virtual void Update(float deltaTime)
        {
        }
    }
}