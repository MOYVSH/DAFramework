using System;
using UnityEngine;

namespace DAPool
{
    public abstract class ObjectPoolBase : IObjectPool
    {
        public abstract Type ObjectType { get; }
        public abstract PoolType PoolType { get; }
        public abstract int Count { get; }
        public abstract int CanReleaseCount { get; }
        public abstract int Capacity { get; set; }

        public abstract void Release();
        public abstract void Release(int toReleaseCount);
        public abstract void ReleaseAllUnused();
        public abstract ObjectInfo[] GetAllObjectInfos();

        /// <summary> 完全关闭并销毁所有对象 </summary>
        public abstract void Shutdown();

        /// <summary> 回收 Component（非泛型入口，由 ObjectPool.Push 调用） </summary>
        internal abstract void UnspawnComponent(Component comp);

        /// <summary> 设置锁定状态（非泛型入口） </summary>
        internal abstract void SetLockedComponent(Component comp, bool locked);

        /// <summary> 获取缓存的 CanvasGroup，无则返回 null </summary>
        internal abstract CanvasGroup GetCanvasGroup(Component comp);
    }
}
