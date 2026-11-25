namespace DAFramework
{
    using System;
    using UnityEngine;

    /// <summary>
    /// 单例基类 —— 纯C#
    /// </summary>
    /// <typeparam name="T">子类类型</typeparam>
    public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        protected virtual bool DontSceneDestory => false;

        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 先在场景中查找
                    _instance = FindAnyObjectByType<T>();
                    if (_instance != null) return _instance;
                    // 未找到则创建
                    var go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (DontSceneDestory) DontDestroyOnLoad(gameObject);
                OnCreate();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            OnUpdate(Time.deltaTime);
        }

        void OnDestroy()
        {
            OnRecycle();
            if (_instance == this) _instance = null;
        }

        protected virtual void OnCreate() { }
        protected virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnRecycle() { }
    }
}

