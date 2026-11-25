using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.DataStructure;

namespace DAFramework.Entity
{
    [Serializable]
    public class GoContainerDictionary : UnitySerializedDictionary<string, GameObject> { } // 使用这个字典快速取得entity子物体的引用
    
    /// <summary>
    /// Entity类
    /// Author： 海豹 / Suomi
    /// 此类为所有在TPS系统中活动且含有数据的物体的基本类，提供粘合组件和自定义生命周期的功能
    /// 此类禁止再继承（请勿去除sealed关键字） 任何功能请使用组件实现
    /// <see cref="EntityComponent"/>>
    /// </summary>
    public sealed class Entity : MonoBehaviour, IEntity
    {
        private int m_Id;
        private string m_EntityAssetName;
        private IEntityGroup m_EntityGroup;
        private bool m_IsActivate = false;

        private Dictionary<Type, object> _sharedStates;

        [SerializeField]
        private GoContainerDictionary m_goContainer;

        [SerializeField] [Tooltip("在场景中预设的Entity请勾选此项，否则不要勾选！不要在Prefab里勾选！（勾选后，实体会在Start函数中向EntityManager注册自己）")]
        private bool _selfInitialize;

        public int Id => m_Id;
        public string EntityAssetName => m_EntityAssetName;
        public object Handle => gameObject;
        
        public IEntityGroup EntityGroup => m_EntityGroup;
        public bool SelfInitialize => _selfInitialize;

        // Entity的逻辑实体
        public List<EntityComponent> m_EntityComponents;

        public Queue<EntityComponent> m_removeEntityComponents;

        private void Start()
        {
            if (!_selfInitialize) return;
            if (m_Id > 0) return;   // 已由工厂/父级注册，避免重复注册

            EntityManager mgr = DA.Entity;
            mgr.RegistEntity(this, gameObject.name, "Default", null);
        }

        public void SetEntityID(int entityId) => m_Id = entityId;

        /// <summary>
        /// 向实体挂载一个共享状态对象（按类型索引，同类型覆盖）。
        /// </summary>
        public void SetShared<T>(T state) where T : class
        {
            if (_sharedStates == null)
                _sharedStates = new Dictionary<Type, object>();
            _sharedStates[typeof(T)] = state;
        }

        /// <summary>
        /// 从实体获取指定类型的共享状态对象，未挂载时返回 null。
        /// </summary>
        public T GetShared<T>() where T : class
        {
            if (_sharedStates != null && _sharedStates.TryGetValue(typeof(T), out var state))
                return (T)state;
            return null;
        }

        public void OnInit(int entityId, string entityAssetName, IEntityGroup entityGroup, object userData)
        {
            m_Id = entityId;
            m_EntityAssetName = entityAssetName;

            m_EntityGroup = entityGroup;

            m_removeEntityComponents = new Queue<EntityComponent>();

            if (m_EntityComponents != null) 
            {
                foreach (var component in m_EntityComponents)
                {
                    component.OnInit(userData,this);
                }
                // try
                // {
                //     foreach (var component in m_EntityComponents)
                //     {
                //         component.OnInit(userData,this);
                //     }
                // }
                // catch (Exception exception)
                // {
                //     Debug.LogErrorFormat("Entity '[{0}]' OnInit with exception '{1}'.", m_Id, exception);
                // }
            }
        }

        public void OnRecycle()
        {
            try
            {
                foreach (var compoment in m_EntityComponents)
                {
                    compoment.OnRecycle();
                    compoment.enabled = false;
                }
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Entity '[{0}]' OnRecycle with exception '{1}'.", m_Id, exception);
            }

            m_Id = 0;
            _sharedStates?.Clear();
        }

        public void OnShow(object userData)
        {
            try
            {
                foreach (var compoment in m_EntityComponents)
                {
                    compoment.OnShow(userData);
                }
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Entity '[{0}]' OnShow with exception '{2}'.", m_Id, exception);
            }
        }

        public void OnHide(bool isShutdown, object userData)
        {
            try
            {
                foreach (var compoment in m_EntityComponents)
                {
                    compoment.OnHide(isShutdown, userData);
                }
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Entity '[{0}]' OnHide with exception '{1}'.", m_Id, exception);
            }
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            if (m_IsActivate == false)
            {
                return;
            }
            
            try
            {
                foreach (var compoment in m_EntityComponents)
                {
                    compoment.OnUpdate();
                }

                OnUpdateComplete();
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Entity '[{0}]' OnUpdate with exception '{1}'.", m_Id, exception);
            }
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void OnUpdateComplete()
        {
            while (m_removeEntityComponents.Count > 0)
            {
                var compoment = m_removeEntityComponents.Dequeue();
                compoment.OnRecycle();
                compoment.enabled = false;
                m_EntityComponents.Remove(compoment);
            }
        }

        public void AddEntityComponent(EntityComponent entityComponent, object userData)
        {
            entityComponent.OnInit(userData, this);
            m_EntityComponents.Add(entityComponent);
        }

        public void RemoveEntityComponent(EntityComponent entityComponent)
        {
            m_removeEntityComponents.Enqueue(entityComponent);
        }

        /// <summary>
        /// 从本实体上获得Entity Component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T RequestEntityComponent<T>() where T : EntityComponent
        {
            T comp = GetComponent<T>();
            if (comp is null)
            {
                Debug.LogError($"Comp {typeof(T)} is not found on Entity {EntityAssetName}");
            }
            return comp;
        }

        /// <summary>
        /// 从本实体Container中的物件上取得Entity Component
        /// </summary>
        /// <param name="goID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T RequestEntityComponent<T>(string goID) where T : EntityComponent
        {
            GameObject go = GetGoReference(goID);
            T comp = null;
            if (go != null)
            {
                comp = go.GetComponent<T>();
                if (comp is null)
                {
                    Debug.LogError($"Comp {nameof(T)} is not found on Entity {go.gameObject.name}");
                }
            }
            return comp;
        }

        public GameObject GetGoReference(string goID)
        {
            if (!m_goContainer.ContainsKey(goID))
            {
                Debug.LogError($"{goID} cannot be found on {EntityAssetName}!");
                return null;
            }

            return m_goContainer[goID];
        }
        
        /// <summary>
        /// 向GoContainer中动态添加一个创建好的Object
        /// </summary>
        /// <param name="goID">string ID,不可重复</param>
        /// <param name="obj">Game Object</param>
        /// <returns>是否添加成功</returns>
        public bool AddGoReference(string goID, GameObject obj)
        {
            if (m_goContainer.ContainsKey(goID))
            {
                Debug.LogError($"Failed to add GO reference. Key {goID} has been used!");
                return false;
            }

            m_goContainer.Add(goID, obj);
            return true;
            
        }
        
        /// <summary>
        /// 从引用字典中去掉一个GameObject。注意Entity本身不负责销毁之，请返回后处理。
        /// </summary>
        /// <param name="goID">>string ID</param>
        /// <returns></returns>
        public GameObject RemoveGoReference(string goID)
        {
            GameObject obj = null;
            if (m_goContainer.TryGetValue(goID, out obj))
            {
                return obj;
            }
            else
            {
                Debug.LogError($"Failed to remove GO reference. Key {goID} do not exist!");
                return null;
            }
        }

        /// <summary>
        /// 找到一个带有某类型Component的引用并去除其引用，返回该组件
        /// </summary>
        /// <param name="goID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T RemoveGoReferenceWithComponent<T>(string goID) where T : EntityComponent
        {
            GameObject obj = null;
            if (m_goContainer.TryGetValue(goID, out obj))
            {
                Component compToRemove;
                if (obj.TryGetComponent(typeof(T), out compToRemove))
                {
                    return compToRemove as T;
                }
                else
                {
                    Debug.LogError($"Failed to remove GO reference. Key {goID} exist, but the component type is not found!");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"Failed to remove GO reference. Key {goID} do not exist!");
                return null;
            }
        }
        
        public void SetActivate(bool isActivate)
        {
            m_IsActivate = isActivate;
        }

        public bool GetActivate()
        {
            return m_IsActivate;
        }
    }
}

