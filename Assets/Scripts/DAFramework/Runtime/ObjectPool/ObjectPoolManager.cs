using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DAFramework;
using Sirenix.OdinInspector;
using System.Linq;

namespace DAPool
{
    [HideMonoScript]
    public class ObjectPoolManager : ModuleSingletonMono<ObjectPoolManager>, IModule
    {
        #region ===== 公开配置 =====
        [Title("物体对象池系统", titleAlignment: TitleAlignments.Centered)]

        [SerializeField, LabelText("池名称 | 容量上限 | 是否UI节点")]
        [ListDrawerSettings(ShowFoldout = false)]
        private List<PoolTypeSetting> _poolSettings;

    #if UNITY_EDITOR
        [Button("同步池枚举", ButtonSizes.Large)]
        private void GeneratePoolEnum()
        {
            var poolNames = _poolSettings
                .Where(p => !string.IsNullOrWhiteSpace(p.PoolName))
                .Select(p => p.PoolName);

            PoolEnumGeneratorTool.Generate(poolNames, "PoolType", "DAPool");
        }
    #endif
        #endregion

        #region ===== 场景节点 =====
        private static Transform m_PoolRoot;
        private Dictionary<PoolType, Transform> m_TypeParents = new();
        private Dictionary<PoolType, PoolTypeSetting> m_TypeSettings = new();
        #endregion

        #region ===== 数据结构 =====
        private readonly Dictionary<GameObject, ObjectPoolBase> m_PrefabPools = new();
        private readonly Dictionary<GameObject, ObjectPoolBase> m_InstanceToPool = new();
        #endregion

        #region ===== 生命周期 =====
        public void OnInit(object createParam)
        {
            BuildTypeSettings();
            InitializeRoots();
        }
        public void OnTermination()
        {
            Shutdown();
        }
        public void OnUpdate(float deltaTime, float unscaledDeltaTime){}
        public void OnLateUpdate(){}
        public void OnFixedUpdate(){}

        public void Shutdown()
        {
            foreach (var pool in m_PrefabPools.Values)
                pool.Shutdown();
            m_PrefabPools.Clear();
            m_InstanceToPool.Clear();
            if (m_PoolRoot != null)
                Destroy(m_PoolRoot.gameObject);
        }

        private void BuildTypeSettings()
        {
            m_TypeSettings.Clear();
            // 按枚举 int 值索引：生成的 PoolType 枚举顺序与 _poolSettings 列表顺序一致
            for (int i = 0; i < _poolSettings.Count; i++)
            {
                m_TypeSettings[(PoolType)i] = _poolSettings[i];
            }
        }

        private void InitializeRoots()
        {
            if (Application.isPlaying)
            {
                var go = new GameObject("[ObjectPoolRoot]");
                DontDestroyOnLoad(go);
                m_PoolRoot = go.transform;

                m_TypeParents.Clear();
                for (int i = 0; i < _poolSettings.Count; i++)
                {
                    var setting = _poolSettings[i];
                    if (string.IsNullOrWhiteSpace(setting.PoolName)) continue;
                    var child = new GameObject(setting.PoolName).transform;
                    child.SetParent(m_PoolRoot);
                    m_TypeParents[(PoolType)i] = child;
                }
            }
            else
            {
                m_PoolRoot = null;
                m_TypeParents.Clear();
            }
        }
        #endregion

        #region ===== 实例追踪 =====
        internal void BindInstance(GameObject go, ObjectPoolBase pool)
            => m_InstanceToPool[go] = pool;

        internal bool UnbindInstance(GameObject go, out ObjectPoolBase pool)
        {
            if (m_InstanceToPool.TryGetValue(go, out pool))
            {
                m_InstanceToPool.Remove(go);
                return true;
            }
            return false;
        }

        internal ObjectPoolBase GetPoolForComponent(GameObject go)
        {
            if (m_InstanceToPool.TryGetValue(go, out var pool))
                return pool;
            return null;
        }
        #endregion

        #region ===== 池创建与查询 =====
        internal static T CreateInstance<T>(GameObject prefab) where T : Component
        {
            var go = Instantiate(prefab);
            var comp = go.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"[ObjectPool] Prefab '{prefab.name}' missing component {typeof(T).Name}");
                Destroy(go);
                return null;
            }
            return comp;
        }

        internal static UniTask<T> InstantiateAsync<T>(GameObject prefab, CancellationToken ct) where T : Component
        {
            var operation = InstantiateAsync(prefab, 1);
            var source = new UniTaskCompletionSource<T>();

            ct.Register(() =>
            {
                operation.Cancel();
                source.TrySetCanceled();
            });

            operation.completed += (_) =>
            {
                if (operation.Result != null && operation.Result.Length > 0)
                {
                    var comp = operation.Result[0].GetComponent<T>();
                    if (comp != null)
                        source.TrySetResult(comp);
                    else
                        source.TrySetException(new InvalidOperationException(
                            $"[ObjectPool] Component {typeof(T).Name} missing on prefab '{prefab.name}'"));
                }
                else
                {
                    source.TrySetException(new InvalidOperationException(
                        $"[ObjectPool] InstantiateAsync returned null for prefab '{prefab.name}'"));
                }
            };

            return source.Task;
        }

        internal static IObjectPool<Transform> GetOrCreatePool(GameObject prefab, PoolType poolType = PoolType.Default, int maxSize = -1)
        {
            var mgr = Instance;
            if (!mgr.m_PrefabPools.TryGetValue(prefab, out var poolBase))
            {
                int capacity = maxSize >= 0 ? maxSize : (mgr.GetPoolTypeSetting(poolType)?.PoolCapacity ?? 5);
                poolBase = new GenericObjectPool<Transform>(capacity, poolType);
                mgr.m_PrefabPools[prefab] = poolBase;
            }
            return (IObjectPool<Transform>)poolBase;
        }

        internal static IObjectPool<Transform> GetPool(GameObject prefab)
        {
            var mgr = Instance;
            mgr.m_PrefabPools.TryGetValue(prefab, out var poolBase);
            return poolBase as IObjectPool<Transform>;
        }
        #endregion

        #region ===== 场景层级与激活控制 =====
        public Transform GetParentTransform(PoolType type)
        {
            if (m_TypeParents.TryGetValue(type, out var parent))
                return parent;
            return m_PoolRoot;
        }

        public void ControlSetActive(Component comp, bool active, ObjectPoolBase poolBase)
        {
            var setting = GetPoolTypeSetting(poolBase.PoolType);
            if (setting != null && setting.IsUIPool)
            {
                var cg = poolBase.GetCanvasGroup(comp);
                if (cg != null)
                {
                    cg.alpha = active ? 1f : 0f;
                    cg.interactable = active;
                    cg.blocksRaycasts = active;
                }
            }
            else
            {
                comp.gameObject.SetActive(active);
            }
        }
        #endregion

        #region ===== 工具方法 =====
        private PoolTypeSetting GetPoolTypeSetting(PoolType type)
        {
            m_TypeSettings.TryGetValue(type, out var setting);
            return setting;
        }
        #endregion
    }
}
