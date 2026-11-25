using UnityEngine;

namespace DAPool
{
    /// <summary> 
    /// 包装对象，记录使用状态、时间、锁定等
    /// </summary>
    internal class ObjectWrapper<T> where T : Component
    {
        /// <summary>
        /// 包裹的真实对象
        /// </summary>
        private T m_Object;

         /// <summary>
         /// 是否正在被外部使用
         /// </summary>
        private bool m_IsInUse;

        /// <summary>
        /// 是否被锁定，锁定的对象不会被自动释放
        /// </summary>
        private bool m_Locked;

        /// <summary>最后使用时间
        /// Spawn 或 Unspawn 时更新
        /// 用于过期判断
        /// </summary>
        private float m_LastUseTime;

        /// <summary>
        /// 被获取的次数
        /// 允许多次获取时用于追踪是否还有外部引用
        /// </summary>
        private int m_SpawnCount;

        /// <summary>
        /// 缓存的CanvasGroup
        /// 仅针对UI类物体作用
        /// </summary>
        private CanvasGroup m_CachedCanvasGroup;

        /// <summary>
        /// 创建包装器实例
        /// </summary>
        public ObjectWrapper(T obj, bool spawned)
        {
            m_Object      = obj;
            m_IsInUse     = spawned;
            m_LastUseTime = Time.realtimeSinceStartup;
            m_SpawnCount  = spawned ? 1 : 0;
        }

        /// <summary>
        /// 得到的真实对象
        /// </summary>
        public T Object => m_Object;

        /// <summary>
        /// 是否正在被使用
        /// </summary>
        public bool IsInUse => m_IsInUse;

        /// <summary>
        /// 是否正在被锁定
        /// </summary>
        public bool Locked
        {
            get => m_Locked;
            set => m_Locked = value;
        }

        /// <summary>
        /// 最后使用时间
        /// </summary>
        public float LastUseTime => m_LastUseTime;

        /// <summary>
        /// 当前被外部引用次数
        /// </summary>
        public int SpawnCount => m_SpawnCount;

        /// <summary>
        /// 缓存的CanvasGroup
        /// </summary>
        public CanvasGroup CachedCanvasGroup
        {
            get
            {
                if (m_CachedCanvasGroup == null)
                {
                    m_CachedCanvasGroup = m_Object.GetComponent<CanvasGroup>();
                    if (m_CachedCanvasGroup == null)
                        m_CachedCanvasGroup = m_Object.gameObject.AddComponent<CanvasGroup>();
                }
                return m_CachedCanvasGroup;
            }
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Spawn()
        {
            m_IsInUse = true;
            m_SpawnCount++;
            m_LastUseTime = Time.realtimeSinceStartup;
            return m_Object;
        }

        /// <summary>
        /// 回收对象到池子
        /// 标记为空闲，但不减少 SpawnCount
        /// </summary>
        public void Unspawn()
        {
            m_IsInUse = false;
        }

        /// <summary>
        /// 彻底释放对象（从池中移除）
        /// 如果 isShutdown 为 true，会调用 Destroy 销毁游戏对象
        /// </summary>
        /// <param name="isShutdown">是否为池关闭操作（是则销毁 GameObject）</param>
        public void Release()
        {
            if (m_Object != null)
            {
                UnityEngine.Object.Destroy(m_Object.gameObject);
            }
            m_Object = null;
        }
    }
}