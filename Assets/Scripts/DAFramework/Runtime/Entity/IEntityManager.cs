using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.Entity
{
    /// <summary>
    /// 实体管理器接口
    /// </summary>
    public interface IEntityManager
    {
        int EntityCount { get; }
        int EntityGroupCount { get; }

        /// <summary>
        /// 显示实体成功事件
        /// </summary>
        event Action ShowEntitySuccess;

        /// <summary>
        /// 显示实体失败事件
        /// </summary>
        event Action ShowEntityFailure;

        /// <summary>
        /// 显示实体更新事件
        /// </summary>
        event Action ShowEntityUpdate;

        /// <summary>
        /// 显示实体时加载依赖资源事件
        /// </summary>
        event Action ShowEntityDependencyAsset;

        /// <summary>
        /// 是否存在实体组
        /// </summary>
        /// <param name="entityGroupName">实体组名称</param>
        /// <returns>是否存在实体组</returns>
        bool HasEntityGroup(string entityGroupName);

        /// <summary>
        /// 获取实体组
        /// </summary>
        /// <param name="entityGroupName">实体组名称</param>
        /// <returns>要获取的实体组</returns>
        IEntityGroup GetEntityGroup(string entityGroupName);
        /// <summary>
        /// 获取所有实体组
        /// </summary>
        /// <returns>所有实体组</returns>
        IEntityGroup[] GetAllEntityGroups();

        /// <summary>
        /// 获取所有实体组
        /// </summary>
        /// <param name="results">所有实体组</param>
        void GetAllEntityGroups(List<IEntityGroup> results);

        /// <summary>
        /// 增加实体组
        /// </summary>
        /// <param name="entityGroupName">实体组名称</param>
        /// <param name="instancePriority">实体实例对象池的优先级</param>
        /// <returns>是否增加实体组成功</returns>
        bool AddEntityGroup(string entityGroupName, int instancePriority);

        /// <summary>
        /// 创建新的实体组
        /// </summary>
        /// <param name="entityGroupName">实体组名称</param>
        /// <param name="instancePriority">实体实例对象池的优先级</param>
        /// <returns>是否增加实体组成功</returns>
        bool CreateEntityGroup(string entityGroupName, int instancePriority);

        /// <summary>
        /// 是否存在实体
        /// </summary>
        /// <param name="entityId">实体编号</param>
        /// <returns>是否存在实体</returns>
        bool HasEntity(int entityId);

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="entityId">实体编号</param>
        /// <returns>要获取的实体</returns>
        IEntity GetEntity(int entityId);

        /// <summary>
        /// 获取所有已加载的实体
        /// </summary>
        /// <returns>所有已加载的实体</returns>
        IEntity[] GetAllLoadedEntities();

        /// <summary>
        /// 获取所有已加载的实体
        /// </summary>
        /// <param name="results">所有已加载的实体</param>
        void GetAllLoadedEntities(List<IEntity> results);

        /// <summary>
        /// 获取所有正在加载实体的编号
        /// </summary>
        /// <returns>所有正在加载实体的编号</returns>
        int[] GetAllLoadingEntityIds();

        /// <summary>
        /// 获取所有正在加载实体的编号
        /// </summary>
        /// <param name="results">所有正在加载实体的编号</param>
        void GetAllLoadingEntityIds(List<int> results);

        /// <summary>
        /// 是否正在加载实体。
        /// </summary>
        /// <param name="entityId">实体编号</param>
        /// <returns>是否正在加载实体</returns>
        bool IsLoadingEntity(int entityId);

        /// <summary>
        /// 是否是合法的实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>实体是否合法</returns>
        bool IsValidEntity(IEntity entity);

        /// <summary>
        /// 展示实体
        /// </summary>
        /// <param name="entityId">实体ID</param>
        /// <param name="entityAssetName">实体资源名</param>
        /// <param name="entityGroupName">实体组名</param>
        void ShowEntity(int entityId, string entityAssetName, string entityGroupName);

        /// <summary>
        /// 展示实体
        /// </summary>
        /// <param name="entityId">实体ID</param>
        /// <param name="entityAssetName">实体资源名</param>
        /// <param name="entityGroupName">实体组名</param>
        /// <param name="userData">用户自定义数据</param>
        void ShowEntity(int entityId, string entityAssetName, string entityGroupName, object userData);

        /// <summary>
        /// 生成新的实体ID
        /// </summary>
        /// <param name="entityGroupName"></param>
        /// <returns></returns>
        int GenerateEntityId(string entityGroupName);

        /// <summary>
        /// 注册实体 已有的地方使用
        /// </summary>
        /// <param name="entity">未初始化的 实体对象</param>
        /// <param name="entityAssetName">实体资源名</param>
        /// <param name="entityGroupName">实体组名</param>
        /// <param name="userData">用户自定义数据</param>
        void RegistEntity(IEntity entity, string entityAssetName, string entityGroupName, object userData);
        
        /// <summary>
        /// 注册实体 已有的地方使用（传ID）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityId"></param>
        /// <param name="entityAssetName"></param>
        /// <param name="entityGroupName"></param>
        /// <param name="userData"></param>
        void RegistEntity(IEntity entity, int entityId, string entityAssetName, string entityGroupName, object userData);

        /// <summary>
        /// 隐藏实体
        /// </summary>
        /// <param name="entityId">实体ID</param>
        void HideEntity(int entityId);

        /// <summary>
        /// 隐藏实体
        /// </summary>
        /// <param name="entityId">实体编号</param>
        /// <param name="userData">用户自定义数据</param>
        void HideEntity(int entityId, object userData);

        /// <summary>
        /// 隐藏实体
        /// </summary>
        /// <param name="entity">实体</param>
        void HideEntity(IEntity entity);

        /// <summary>
        /// 隐藏实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="userData">用户自定义数据</param>
        void HideEntity(IEntity entity, object userData);

        /// <summary>
        /// 隐藏所有已加载的实体
        /// </summary>
        void HideAllLoadedEntities();

        /// <summary>
        /// 隐藏所有已加载的实体
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        void HideAllLoadedEntities(object userData);

        /// <summary>
        /// 隐藏所有正在加载的实体
        /// </summary>
        void HideAllLoadingEntities();
    }
}

