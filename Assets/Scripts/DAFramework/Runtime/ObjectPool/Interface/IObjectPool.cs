using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DAPool
{
    [Serializable]
    public class PoolTypeSetting
    {
        [HideLabel, HorizontalGroup("row")]
        public string PoolName = "Default";
        [HideLabel, HorizontalGroup("row", Width = 50)]
        public int PoolCapacity = 10;
        [HideLabel, HorizontalGroup("row", Width = 20)]
        public bool IsUIPool = false;
    }

    /// <summary>
    /// 对象池中单个对象的状态信息，供外部查询与调试。
    /// </summary>
    public struct ObjectInfo
    {
        public string Name;
        public bool   Locked;
        public float  LastUseTime;
        public int    SpawnCount;
    }

    /// <summary>
    /// 池化对象生命周期回调。对象入池/出池时自动调用（可选实现）。
    /// </summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnUnspawn();
    }

    /// <summary>
    /// 非泛型对象池接口 —— 类型无关的公共操作。
    /// </summary>
    public interface IObjectPool
    {
        Type ObjectType { get; }
        PoolType PoolType { get; }
        int Count { get; }
        int CanReleaseCount { get; }
        int Capacity { get; set; }
        void Release();
        void Release(int toReleaseCount);
        void ReleaseAllUnused();
        ObjectInfo[] GetAllObjectInfos();
    }

    /// <summary>
    /// 泛型对象池接口。
    /// </summary>
    public interface IObjectPool<T> : IObjectPool where T : Component
    {
        void Spawn(T obj, bool spawned);
        bool CanSpawn();
        T Spawn();
        void Unspawn(T obj);
        void SetLocked(T obj, bool locked);
        bool ReleaseObject(T obj);
    }
}
