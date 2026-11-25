using DAFramework.ReferencePool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.Buff
{
    /// <summary>
    /// Buff基类
    /// </summary>
    public class BuffBase : IReference
    {
        #region 属性  
        /// <summary>
        /// 拥有当前Buff的游戏对象 这里我准备绑定在数据层上
        /// </summary>
        public IAddableBuff Owner { get; internal set; }

        /// <summary>
        /// Buff配置信息
        /// </summary>
        public BuffConfigData BuffConfig { get; internal set; }

        /// <summary>
        /// Buff激活时间
        /// </summary>
        public float ActiveTime { get; private set; }
        /// <summary>
        /// Buff持续时间
        /// </summary>
        public float DurationTime { get; set; }

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress 
        {
            get
            {
                if (DurationTime <= 0)
                    return -1;

                return ActiveTime / DurationTime;
            }
        }

        public int Count { get; set; }
        
        #endregion

        #region 方法
        internal void Update(float deltaTime)
        {
            ActiveTime += deltaTime;
            OnUpdate(deltaTime);
        }

        /// <summary>
        /// Buff被添加时调用
        /// </summary>
        public virtual void OnAdd() { }

        /// <summary>
        /// Buff被移除时调用
        /// </summary>
        public virtual void OnRemove() { }

        public virtual void OnAddCount() { }
        
        public virtual void OnResetTime() { }
        
        /// <summary>
        /// Buff更新时调用
        /// </summary>
        public virtual void OnUpdate(float deltaTime) { }

        public void Clear()
        {
            Reset();
        }

        public void Reset()
        {
            DurationTime = BuffConfig.m_Buff_Duration;
            ActiveTime = 0;
            Owner = null;
        }
        
        public void ResetTime()
        {
            ActiveTime = 0;
            OnResetTime();
        }
        
        public void AddCount()
        {
            Count++;
            ActiveTime = 0;
            OnAddCount();
        }
        #endregion
    }
}