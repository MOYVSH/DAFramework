using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.Entity
{
    /// <summary>
    /// 实体组接口
    /// </summary>
    public interface IEntityGroup
    {
        /// <summary>
        /// 获取实体组名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取实体组中实体数量
        /// </summary>
        int EntityCount { get; }

        /// <summary>
        /// 获得实体组在轮询时候的优先级
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// EntityGroup的ID用于生成自增的EntityID，并限制EntityGroup中存储的Entity数量
        /// </summary>
        int GruphId { get; }

        /// <summary>
        /// 实体组中是否存在实体
        /// </summary>
        /// <param name="EntityID">实体ID</param>
        /// <returns></returns>
        bool HasEntity(int EntityID);

        /// <summary>
        /// 从实体组中获取实体
        /// </summary>
        /// <param name="EntityID">实体ID</param>
        /// <returns>要获取的实体</returns>
        IEntity GetEntity(int EntityID);

        /// <summary>
        /// 获取实体组中的所有实体
        /// </summary>
        /// <returns>实体组中的所有实体</returns>
        IEntity[] GetAllEntitys();

        /// <summary>
        /// 从实体组中获取所有实体。
        /// </summary>
        /// <param name="results">实体组中的所有实体。</param>
        void GetAllEntities(List<IEntity> results);

        /// <summary>
        /// 向实体组中添加实体
        /// </summary>
        /// <param name="entity"></param>
        void AddEntity(IEntity entity);

        /// <summary>
        /// 通过 ID 移除实体组中的实体
        /// </summary>
        /// <param name="EntityID"></param>
        void RemoveEntity(int EntityID);

        void RemoveEntity(IEntity entity);
    }
}

