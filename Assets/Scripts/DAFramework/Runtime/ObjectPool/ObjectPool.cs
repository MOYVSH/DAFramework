using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DAPool
{
    public static class ObjectPool
    {
        #region ===== 预加载 =====
        public static void Preload(GameObject prefab, int initCount, int maxCount = -1, Transform parent = null, PoolType poolType = default)
        {
            if (prefab == null) return;
            var pool = ObjectPoolManager.GetOrCreatePool(prefab, poolType, maxCount);
            var poolBase = (ObjectPoolBase)pool;
            // 确保池容量能容纳 count 个空闲对象 + 无法释放的对象（使用中/锁定）
            int freeCount = poolBase.CanReleaseCount;
            int nonReleasable = poolBase.Count - freeCount;
            poolBase.Capacity = Mathf.Max(poolBase.Capacity, nonReleasable + initCount);
            // 扩容可能触发 Release 回收，重新读取空闲数
            freeCount = poolBase.CanReleaseCount;
            int toCreate = Mathf.Max(0, initCount - freeCount);
            for (int i = 0; i < toCreate; i++)
            {
                var trans = ObjectPoolManager.CreateInstance<Transform>(prefab);
                if (trans == null) continue;
                trans.SetParent(parent != null ? parent : ObjectPoolManager.Instance.GetParentTransform(poolBase.PoolType), false);
                pool.Spawn(trans, false);
                ObjectPoolManager.Instance.ControlSetActive(trans, false, poolBase);
            }
        }
        #endregion

        #region ===== 获取 =====
        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, PoolType poolType = default)
            => GetInternal<Transform>(prefab, position, rotation, parent, poolType).gameObject;

        public static GameObject Get(GameObject prefab, Transform parent, PoolType poolType = default)
            => GetInternal<Transform>(prefab, Vector3.zero, Quaternion.identity, parent, poolType).gameObject;

        public static T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, PoolType poolType = default) where T : Component
            => GetInternal<T>(prefab, position, rotation, parent, poolType);

        public static T Get<T>(GameObject prefab, Transform parent, PoolType poolType = default) where T : Component
            => GetInternal<T>(prefab, Vector3.zero, Quaternion.identity, parent, poolType);

        private static T GetInternal<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, PoolType poolType) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPool] Prefab is null.");
                return null;
            }
            var pool = ObjectPoolManager.GetOrCreatePool(prefab, poolType);
            Transform trans;
            if (pool.CanSpawn())
            {
                trans = pool.Spawn();
            }
            else
            {
                trans = ObjectPoolManager.CreateInstance<Transform>(prefab);
                if (trans == null) return null;
                pool.Spawn(trans, true);
            }
            T comp = trans.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"[ObjectPool] Prefab '{prefab.name}' missing component {typeof(T).Name}.");
                return null;
            }
            if (comp is IPoolable poolable)
                poolable.OnSpawn();
            SetupAndActivate(comp, (ObjectPoolBase)pool, position, rotation, parent, ((ObjectPoolBase)pool).PoolType);
            return comp;
        }
        #endregion

        #region ===== 异步获取 =====
        public static UniTask<T> GetAsync<T>(GameObject prefab, Transform parent = null, PoolType poolType = default, CancellationToken cancellationToken = default) where T : Component
            => GetAsync<T>(prefab, Vector3.zero, Quaternion.identity, parent, poolType, cancellationToken);

        public static UniTask<T> GetAsync<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, PoolType poolType = default, CancellationToken cancellationToken = default) where T : Component
        {
            if (prefab == null)
                return UniTask.FromResult<T>(null);

            var pool = ObjectPoolManager.GetOrCreatePool(prefab, poolType);
            var poolBase = (ObjectPoolBase)pool;

            if (pool.CanSpawn())
            {
                Transform trans = pool.Spawn();
                T comp = trans.GetComponent<T>();
                if (comp == null) return UniTask.FromResult<T>(null);
                if (comp is IPoolable p)
                    p.OnSpawn();
                SetupAndActivate(comp, poolBase, position, rotation, parent, poolBase.PoolType);
                return UniTask.FromResult(comp);
            }

            return GetAsyncCore<T>(prefab, pool, position, rotation, parent, cancellationToken);
        }

        private static async UniTask<T> GetAsyncCore<T>(
            GameObject prefab, IObjectPool<Transform> pool,
            Vector3 position, Quaternion rotation, Transform parent,
            CancellationToken ct) where T : Component
        {
            ct.ThrowIfCancellationRequested();
            Transform trans = await ObjectPoolManager.InstantiateAsync<Transform>(prefab, ct);
            ct.ThrowIfCancellationRequested();

            if (trans == null)
                return default;

            T comp = trans.GetComponent<T>();
            if (comp == null)
                return default;

            if (comp is IPoolable p)
                p.OnSpawn();
            var poolBase = (ObjectPoolBase)pool;
            pool.Spawn(trans, true);
            SetupAndActivate(comp, poolBase, position, rotation, parent, poolBase.PoolType);
            return comp;
        }
        #endregion

        #region ===== 工具方法 =====
        private static void SetupAndActivate<T>(T comp, ObjectPoolBase poolBase, Vector3 position, Quaternion rotation, Transform parent, PoolType poolType) where T : Component
        {
            Transform trans = comp.transform;
            Transform targetParent = parent != null ? parent : ObjectPoolManager.Instance.GetParentTransform(poolType);
            trans.SetParent(targetParent);
            trans.localScale = Vector3.one;
            trans.SetPositionAndRotation(position, rotation);
            ObjectPoolManager.Instance.BindInstance(comp.gameObject, poolBase);
            // 传 Transform 以兼容池内 CanvasGroup 查找
            ObjectPoolManager.Instance.ControlSetActive(trans, true, poolBase);
        }
        #endregion

        #region ===== 回收 =====
        public static void Push(GameObject go, Transform parent = null)
        {
            if (go == null) return;
            PushInternal(go, parent);
        }

        public static void Push(Component component, Transform parent = null)
        {
            if (component == null) return;
            PushInternal(component.gameObject, parent);
        }

        private static void PushInternal(GameObject go, Transform parent)
        {
            var mgr = ObjectPoolManager.Instance;
            if (mgr.UnbindInstance(go, out var poolBase))
            {
                // 通知所有 IPoolable 组件回收
                foreach (var poolable in go.GetComponents<IPoolable>())
                    poolable.OnUnspawn();
                poolBase.UnspawnComponent(go.transform);
                PoolType type = poolBase.PoolType;
                mgr.ControlSetActive(go.transform, false, poolBase);
                go.transform.SetParent(parent != null ? parent : mgr.GetParentTransform(type), false);
            }
            else
            {
                // 未追踪的对象仅修改父节点，不销毁（由调用者决定生命周期）
                if (parent != null)
                    go.transform.SetParent(parent, false);
            }
        }

        /// <summary>
        /// 回收 parent 下所有“属于指定池类型”的对象。
        /// 必须按池类型过滤：子层级里可能还挂着别的池的对象（例如 UI 池的准星），
        /// 无差别回收会把仍在使用的对象塞回池里（表现为 CanvasGroup.alpha 被置 0、父级被挪到池根节点）。
        /// </summary>
        public static void Clear(Transform parent, PoolType? onlyPoolType = null)
        {
            if (parent == null) return;

            var mgr = ObjectPoolManager.Instance;
            if (mgr == null) return;

            var allChildren = parent.GetComponentsInChildren<Transform>(includeInactive: true);
            for (int i = 0; i < allChildren.Length; i++)
            {
                var go = allChildren[i].gameObject;
                if (go == parent.gameObject) continue;

                ObjectPoolBase pool = mgr.GetPoolForComponent(go);
                if (pool == null) continue;
                if (onlyPoolType.HasValue && pool.PoolType != onlyPoolType.Value) continue;

                PushInternal(go, null);
            }
        }
        #endregion

        #region ===== 池配置 =====
        public static void SetCapacity(GameObject prefab, int capacity)
        {
            var poolBase = (ObjectPoolBase)ObjectPoolManager.GetPool(prefab);
            if (poolBase != null)
                poolBase.Capacity = capacity;
        }

        public static void ReleaseAllUnused(GameObject prefab)
        {
            var poolBase = (ObjectPoolBase)ObjectPoolManager.GetPool(prefab);
            poolBase?.ReleaseAllUnused();
        }
        #endregion
    }
}
