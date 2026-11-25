namespace DAFramework
{
    using System;

    /// <summary>
    /// 单例基类 —— 纯C#
    /// </summary>
    /// <typeparam name="T">子类类型</typeparam>
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
        // 防止外部通过构造函数实例化
        protected Singleton() { }
    }
}

