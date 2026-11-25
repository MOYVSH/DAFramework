using System;
using System.Collections.Generic;
using UnityEngine;

namespace DAPool
{
    internal class GenericObjectPool<T> : ObjectPoolBase, IObjectPool<T> where T : Component
    {
        #region ===== 存储结构 =====
        private readonly Dictionary<T, ObjectWrapper<T>> m_ObjectToWrapper = new();
        private readonly Stack<ObjectWrapper<T>> m_FreeObjects = new();
        #endregion

        #region ===== 配置 =====
        private int m_Capacity;
        private int m_CachedCanReleaseCount;
        #endregion

        #region ===== 缓存列表 =====
        private readonly List<T> m_CachedCanReleaseObjects = new();
        #endregion

        #region ===== 初始化 =====
        public GenericObjectPool(int capacity, PoolType poolType)
        {
            m_Capacity = Mathf.Max(0, capacity);
            PoolType = poolType;
        }
        #endregion

        #region ===== 属性实现 =====
        public override PoolType PoolType { get; }
        public override Type ObjectType => typeof(T);
        public override int Count => m_ObjectToWrapper.Count;
        public override int CanReleaseCount => m_CachedCanReleaseCount;

        public override int Capacity
        {
            get => m_Capacity;
            set
            {
                if (value < 0) return;
                m_Capacity = value;
                Release();
            }
        }
        #endregion

        #region ===== 公开方法 =====
        public void Spawn(T obj, bool spawned)
        {
            if (obj == null) return;

            var wrapper = new ObjectWrapper<T>(obj, spawned);
            m_ObjectToWrapper[obj] = wrapper;

            if (!spawned)
            {
                m_FreeObjects.Push(wrapper);
                m_CachedCanReleaseCount++;
            }
        }

        public bool CanSpawn() => m_FreeObjects.Count > 0;

        public T Spawn()
        {
            while (m_FreeObjects.Count > 0)
            {
                var wrapper = m_FreeObjects.Pop();
                if (wrapper.Object == null)
                    continue;
                if (!wrapper.IsInUse)
                {
                    m_CachedCanReleaseCount--;
                    var obj = wrapper.Spawn();
                    return obj;
                }
            }
            return null;
        }

        public void Unspawn(T obj)
        {
            if (obj == null) return;
            if (m_ObjectToWrapper.TryGetValue(obj, out var wrapper))
            {
                if (!wrapper.IsInUse) return; // already released
                wrapper.Unspawn();
                m_FreeObjects.Push(wrapper);
                m_CachedCanReleaseCount++;

                if (CanReleaseCount > m_Capacity)
                    Release(CanReleaseCount - m_Capacity);
            }
        }

        public void SetLocked(T obj, bool locked)
        {
            if (m_ObjectToWrapper.TryGetValue(obj, out var wrapper))
            {
                bool wasLocked = wrapper.Locked;
                wrapper.Locked = locked;
                // 仅在非使用时调整缓存计数
                if (!wrapper.IsInUse && wasLocked != locked)
                    m_CachedCanReleaseCount += locked ? -1 : 1;
            }
        }

        public bool ReleaseObject(T obj)
        {
            if (obj == null) return false;
            if (!m_ObjectToWrapper.TryGetValue(obj, out var wrapper)) return false;
            if (wrapper.IsInUse || wrapper.Locked) return false;

            m_ObjectToWrapper.Remove(obj);
            m_CachedCanReleaseCount--;
            wrapper.Release();
            return true;
        }

        public override void Release() => Release(Count - m_Capacity);

        public override void Release(int toReleaseCount)
        {
            if (toReleaseCount <= 0) return;

            GetCanReleaseObjects(m_CachedCanReleaseObjects);
            if (m_CachedCanReleaseObjects.Count == 0) return;

            int toRelease = Mathf.Min(toReleaseCount, m_CachedCanReleaseObjects.Count);

            // LRU: 按 LastUseTime 升序，最旧的先释放
            m_CachedCanReleaseObjects.Sort((a, b) =>
                m_ObjectToWrapper[a].LastUseTime.CompareTo(m_ObjectToWrapper[b].LastUseTime));

            for (int i = 0; i < toRelease; i++)
                ReleaseObject(m_CachedCanReleaseObjects[i]);

            RebuildFreeStack();
        }

        public override void ReleaseAllUnused()
        {
            GetCanReleaseObjects(m_CachedCanReleaseObjects);
            foreach (var obj in m_CachedCanReleaseObjects)
                ReleaseObject(obj);
            RebuildFreeStack();
        }

        public override ObjectInfo[] GetAllObjectInfos()
        {
            var results = new ObjectInfo[m_ObjectToWrapper.Count];
            int i = 0;
            foreach (var wrapper in m_ObjectToWrapper.Values)
            {
                results[i++] = new ObjectInfo
                {
                    Name = wrapper.Object != null ? wrapper.Object.name : "(destroyed)",
                    Locked = wrapper.Locked,
                    LastUseTime = wrapper.LastUseTime,
                    SpawnCount = wrapper.SpawnCount
                };
            }
            return results;
        }
        #endregion

        #region ===== 内部方法 =====
        internal override void UnspawnComponent(Component comp)
        {
            if (comp is T obj)
                Unspawn(obj);
        }

        internal override void SetLockedComponent(Component comp, bool locked)
        {
            if (comp is T obj)
                SetLocked(obj, locked);
        }

        internal override CanvasGroup GetCanvasGroup(Component comp)
        {
            if (comp is T obj && m_ObjectToWrapper.TryGetValue(obj, out var wrapper))
                return wrapper.CachedCanvasGroup;
            return null;
        }
        #endregion

        #region ===== 生命周期 =====
        public override void Shutdown()
        {
            foreach (var wrapper in m_ObjectToWrapper.Values)
                wrapper.Release();
            m_FreeObjects.Clear();
            m_ObjectToWrapper.Clear();
            m_CachedCanReleaseObjects.Clear();
            m_CachedCanReleaseCount = 0;
        }
        #endregion

        #region ===== 私有方法 =====
        private void GetCanReleaseObjects(List<T> results)
        {
            results.Clear();
            foreach (var wrapper in m_ObjectToWrapper.Values)
            {
                if (!wrapper.IsInUse && !wrapper.Locked)
                    results.Add(wrapper.Object);
            }
        }

        private void RebuildFreeStack()
        {
            m_FreeObjects.Clear();
            foreach (var wrapper in m_ObjectToWrapper.Values)
            {
                if (!wrapper.IsInUse && !wrapper.Locked)
                    m_FreeObjects.Push(wrapper);
            }
        }

        #endregion
    }
}
