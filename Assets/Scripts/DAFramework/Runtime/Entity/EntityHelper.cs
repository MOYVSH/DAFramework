using System;
using System.Collections.Generic;
using UnityEngine;
using DAPool;
using UnityEngine.SceneManagement;

namespace DAFramework.Entity
{
    public static class EntityHelper
    {
        public static Entity CreateEntity(
            GameObject prefab,
            out int entityId,
            Vector3 position = default,
            Quaternion rotation = default,
            string groupName = "Default")
        {
            entityId = 0;

            if (prefab == null)
            {
                Debug.LogError("Prefab 不能为空!");
                return null;
            }

            Entity entity = ObjectPool.Get<Entity>(
                prefab,
                position,
                rotation
            );

            if (entity == null)
                return null;

            entity.transform.SetParent(null);
            SceneManager.MoveGameObjectToScene(entity.gameObject, SceneManager.GetActiveScene());

            if (!entity.SelfInitialize)
            {
                EntityManager mgr = EntityManager.Instance;
                entityId = mgr.GenerateEntityId(groupName);
                entity.SetEntityID(entityId);
            }

            return entity;
        }

        public static void CreateEntity(string addressName, string GroupName)
        {
            // TODO: 资源热更方案待定——原通过 Addressables 异步加载并实例化实体的逻辑已移除。
            // 新资源系统接入后，于此加载 addressName 对应资源，实例化后注册 Entity。
            Debug.LogWarning($"EntityHelper.CreateEntity(\"{addressName}\") 暂未实现：资源热更方案待接入。");
        }

        public static void ActiveEntity(Entity entity, int entityID, string addressName, string groupName = "Default", bool isActive = true)
        {
            if (entity == null)
                return;

            if (!entity.SelfInitialize)
            {
                EntityManager mgr = EntityManager.Instance;
                mgr.RegistEntity(entity, entityID, addressName, groupName, null);

                Debug.Log($"成功创建Entity <color=yellow>{entity.gameObject.name}</color>");
            }

            entity.SetActivate(true);

            // 激活组件对应的 GameObject
            entity.gameObject.SetActive(isActive);
        }
        public static void ReleaseEntity(IEntity entity)
        {
            // TODO: 资源热更方案待定——原通过 Addressables 释放实体资源的逻辑已移除。
            // 新资源系统接入后，于此释放 entity.EntityAssetName 对应资源。
        }

        public static bool FastAddEntityComponent(Entity entity, EntityComponent entityComponent, object userData)
        { 
            if (entity == null) 
            {
                Debug.LogError("Unable to Add EntityComponent for a null Entity in EntityHelper.");
                return false;
            }

            if (entityComponent == null) 
            {
                Debug.LogError("Unable to Add a null EntityComponent for Entity in EntityHelper.");
                return false;
            }

            entity.AddEntityComponent(entityComponent, userData);

            return true;
        }

        public static bool FastRemoveEntityComponent(Entity entity, EntityComponent entityComponent)
        {
            if (entity == null)
            {
                Debug.LogError("Unable to Remove EntityComponent from a null Entity in EntityHelper.");
                return false;
            }

            if (entityComponent == null)
            {
                Debug.LogError("Unable to Remove a null EntityComponent in EntityHelper.");
                return false;
            }

            entity.RemoveEntityComponent(entityComponent);

            return true;
        }

        /// <summary>
        /// 获取特定的EntityComponent
        /// </summary>
        /// <typeparam name="T">要获取的EntityComponent的类型</typeparam>
        /// <param name="components">Entity中缓存的EntityComponent</param>
        /// <returns>获取到的EntityComponent</returns>
        public static List<EntityComponent> GetEntityComponentByType<T>(List<EntityComponent> components)
        {
            List<EntityComponent> result = new List<EntityComponent>();

            if (components == null)
            {
                Debug.LogError("Unable to Iterate through a EntityComponents in EntityHelper.");
                return result;
            }

            foreach (EntityComponent component in components) 
            {
                if (component.GetType() == typeof(T))
                {
                    result.Add(component);
                }
            }
            return result;
        }
        public static Entity FastGetEntity(GameObject go)
        {
            Entity entity = go.GetComponent<Entity>();
            return entity;
        }

        public static EntityComponent FastGetEntityComponent<T>(GameObject go)
        {
            Entity entity = go.GetComponent<Entity>();
            if (entity == null)
            {
                Debug.LogError($"Unable to Get EntityComponent from GameObject {go.name} in EntityHelper because it does not have an Entity component.");
                return null;
            }
            
            EntityComponent result = null;
            foreach (EntityComponent component in entity.m_EntityComponents) 
            {
                if (component.GetType() == typeof(T))
                {
                    result = component;
                    
                    break;
                }
            }
            return result;
        }

        public static EntityComponent FastGetEntityComponent<T>(Entity entity)
        {
            if (entity == null)
            {
                Debug.LogError("Unable to Get EntityComponent from a null Entity in EntityHelper.");
                return null;
            }

            EntityComponent result = null;
            foreach (EntityComponent component in entity.m_EntityComponents)
            {
                if (component.GetType() == typeof(T))
                {
                    result = component;

                    break;
                }
            }

            return result;
        }

        public static bool TryGetEventBus(int entityId, out EntityEventBusComp eventBus)
        {
            eventBus = null;
            var entity = DA.Entity.GetEntity(entityId) as Entity;
            if (entity == null)
            {
                Debug.LogError($"System: Entity with ID {entityId} not found.");
                return false;
            }

            var ec = FastGetEntityComponent<EntityEventBusComp>(entity);
            if (ec == null)
            {
                Debug.LogError($"System: Entity with ID {entityId} does not have an EntityEventBusComp.");
                return false;
            }

            eventBus = ec as EntityEventBusComp;
            if (eventBus == null)
            {
                Debug.LogError($"System: Entity with ID {entityId} has an invalid EntityEventBusComp.");
                return false;
            }

            if (eventBus.EvtHandler == null)
            {
                Debug.LogError($"System: Entity with ID {entityId} has no EvtHandler on its EntityEventBusComp.");
                return false;
            }

            return true;
        } 
    }
}
