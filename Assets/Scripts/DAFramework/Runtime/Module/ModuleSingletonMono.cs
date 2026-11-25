using UnityEngine;

namespace DAFramework
{
    public abstract class ModuleSingletonMono<T> : MonoBehaviour where T : class, IModule
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError($"模块 {typeof(T)} 未创建。");
                return _instance;
            }
        }
        
        protected virtual bool UseDontDestroyOnLoad => true;

        private void Awake()
        {
            //防止创建多余单例
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this as T;

            if (transform.parent != null)
                transform.SetParent(null);

            if (UseDontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }
        
        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }
}