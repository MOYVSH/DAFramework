using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DAPool
{
    public static class DecalPool
    {
        private const PoolType FixedPoolType = PoolType.Effect;

        #region ===== 预加载 =====
        public static void Preload(GameObject prefab, int initCount, int maxCount = -1, Transform parent = null)
        {
            if(prefab == null) return;
            ObjectPool.Preload(prefab, initCount, maxCount, parent, FixedPoolType);
        }
        #endregion

        #region ===== 同步获取 =====
        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, float autoRecycleAfter = 0f)
        {
            var go = ObjectPool.Get(prefab, position, rotation, parent, FixedPoolType);
            if (go != null)
                TryAutoRecycle(go.transform, autoRecycleAfter);
            return go;
        }

        public static GameObject Get(GameObject prefab, Transform parent = null, float autoRecycleAfter = 0f)
            => Get(prefab, Vector3.zero, Quaternion.identity, parent, autoRecycleAfter);

        public static T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, float autoRecycleAfter = 0f) where T : Component
        {
            T comp = ObjectPool.Get<T>(prefab, position, rotation, parent, FixedPoolType);
            if (comp != null)
                TryAutoRecycle(comp, autoRecycleAfter);
            return comp;
        }

        public static T Get<T>(GameObject prefab, Transform parent = null, float autoRecycleAfter = 0f) where T : Component
            => Get<T>(prefab, Vector3.zero, Quaternion.identity, parent, autoRecycleAfter);
        #endregion

        #region ===== 异步获取 =====
        public static UniTask<T> GetAsync<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, float autoRecycleAfter = 0f, CancellationToken ct = default) where T : Component
        {
            var task = ObjectPool.GetAsync<T>(prefab, position, rotation, parent, FixedPoolType, ct);
            if (autoRecycleAfter <= 0f)
                return task;
            return GetAsyncWithRecycle(task, autoRecycleAfter);
        }

        public static UniTask<T> GetAsync<T>(GameObject prefab, Transform parent = null, float autoRecycleAfter = 0f, CancellationToken ct = default) where T : Component
            => GetAsync<T>(prefab, Vector3.zero, Quaternion.identity, parent, autoRecycleAfter, ct);

        private static async UniTask<T> GetAsyncWithRecycle<T>(UniTask<T> task, float autoRecycleAfter) where T : Component
        {
            T comp = await task;
            if (comp != null)
                AutoRecycleAsync(comp, autoRecycleAfter).Forget();
            return comp;
        }
        #endregion

        #region ===== 回收 =====
        public static void Push(GameObject go, Transform parent = null)
            => ObjectPool.Push(go, parent);

        public static void Push(Component component, Transform parent = null)
            => ObjectPool.Push(component, parent);

        public static void Clear(Transform parent)
            => ObjectPool.Clear(parent, FixedPoolType);
        #endregion

        #region ===== 自动回收 =====
        private static void TryAutoRecycle(Component comp, float delaySeconds)
        {
            if (delaySeconds > 0f)
                AutoRecycleAsync(comp, delaySeconds).Forget();
        }

        private static async UniTaskVoid AutoRecycleAsync<T>(T comp, float delaySeconds) where T : Component
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));
            if (ObjectPoolManager.Instance.GetPoolForComponent(comp.gameObject) != null)
                ObjectPool.Push(comp);
        }
        #endregion
    }
}
