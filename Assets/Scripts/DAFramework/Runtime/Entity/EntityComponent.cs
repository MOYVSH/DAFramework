using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.Entity
{
    /// <summary>
    /// 实体逻辑类
    /// </summary>
    public abstract class EntityComponent : MonoBehaviour
    {
        private bool m_Available = false;
        private bool m_Visible = false;
        private Entity m_Entity = null;
        private int m_OriginalLayer = 0;

        /// <summary>
        /// 获取实体。
        /// </summary>
        public Entity Entity => m_Entity;

        /// <summary>
        /// 获取或设置实体名称。
        /// </summary>
        public string Name
        {
            get
            {
                return gameObject.name;
            }
            set
            {
                gameObject.name = value;
            }
        }

        /// <summary>
        /// 获取实体是否可用。
        /// </summary>
        public bool Available => m_Available;

        /// <summary>
        /// 获取或设置实体是否可见。
        /// </summary>
        public bool Visible
        {
            get
            {
                return m_Available && m_Visible;
            }
            set
            {
                if (!m_Available)
                {
                    Debug.LogWarning($"Entity '{Name}' is not available.");
                    return;
                }

                m_Visible = value;
                InternalSetVisible(value);
            }
        }

        /// <summary>
        /// 实体初始化。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="owner">调用此函数的实体，通常为该组件所有者</param>
        protected internal virtual void OnInit(object userData, Entity owner)
        {
            m_Entity = owner;
            m_OriginalLayer = gameObject.layer;
        }


        /// <summary>
        /// 实体回收。
        /// </summary>
        protected internal virtual void OnRecycle()
        {
        }

        /// <summary>
        /// 实体显示。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnShow(object userData)
        {
            m_Available = true;
            Visible = true;
        }

        /// <summary>
        /// 实体隐藏。
        /// </summary>
        /// <param name="isShutdown">是否是关闭实体管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnHide(bool isShutdown, object userData)
        {
            gameObject.SetLayerRecursively(m_OriginalLayer);
            Visible = false;
            m_Available = false;
        }

        /// <summary>
        /// 实体轮询。
        /// </summary>
        protected internal virtual void OnUpdate()
        {
        }

        /// <summary>
        /// 设置实体的可见性。
        /// </summary>
        /// <param name="visible">实体的可见性。</param>
        protected virtual void InternalSetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}

