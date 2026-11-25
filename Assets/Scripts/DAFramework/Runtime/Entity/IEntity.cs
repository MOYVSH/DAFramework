using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.Entity
{
    /// <summary>
    /// 实体接口
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// 实体编号
        /// </summary>
        int Id { get; }

        /// <summary>
        /// 获取实体资源名称。
        /// </summary>
        string EntityAssetName
        {
            get;
        }


        /// <summary>
        /// 获取实体实例
        /// </summary>
        object Handle { get; }

        /// <summary>
        /// 获取实体所属的实体组
        /// </summary>
        IEntityGroup EntityGroup { get; }

        /// <summary>
        /// 实体初始化
        /// </summary>
        /// <param name="entityId">实体编号</param>
        /// <param name="entityGroup">实体所属实体组</param>
        /// <param name="userData">用户自定义数据</param>
        void OnInit(int entityId, string entityAssetName, IEntityGroup entityGroup, object userData);

        /// <summary>
        /// 实体回收
        /// </summary>
        void OnRecycle();

        /// <summary>
        /// 展示实体
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        void OnShow(object userData);

        /// <summary>
        /// 隐藏实体
        /// </summary>
        /// <param name="isShutdown">是否在关闭实体管理器时触发</param>
        /// <param name="userData">用户自定义数据</param>
        void OnHide(bool isShutdown, object userData);

        /// <summary>
        /// 实体轮询
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
        void OnUpdate(float elapseSeconds, float realElapseSeconds);

        void SetActivate(bool activate);
        
        bool GetActivate();
        
        GameObject GetGameObject();
    }
}
