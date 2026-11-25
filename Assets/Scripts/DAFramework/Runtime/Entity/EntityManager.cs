using System;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.Entity
{
    /// <summary>
    /// 实体管理器
    /// </summary>
    [UpdateRefresh]
    public sealed partial class EntityManager : ModuleSingletonMono<EntityManager>, IEntityManager, IModule
    {
        // 用于在非运行状态提前设置好一些GrouphEntity
        public List<EntityGroupInfo> entityGrouphInfos = new List<EntityGroupInfo>();

        private Dictionary<int, EntityInfo> m_EntityInfos;
        private Dictionary<string, EntityGroup> m_EntityGroups;
        private Dictionary<int, int> m_EntitiesBeingLoaded;
        private Queue<EntityInfo> m_RecycleQueue;
        private bool m_IsShutdown;

        public int EntityCount => m_EntityInfos.Count;
        public int EntityGroupCount => m_EntityGroups.Count;

        public event Action ShowEntitySuccess;
        public event Action ShowEntityFailure;
        public event Action ShowEntityUpdate;
        public event Action ShowEntityDependencyAsset;


        public void OnInit(object createParam)
        {
            m_EntityInfos = new Dictionary<int, EntityInfo>();
            m_EntityGroups = new Dictionary<string, EntityGroup>();
            m_EntitiesBeingLoaded = new Dictionary<int, int>();
            m_RecycleQueue = new Queue<EntityInfo>();

            m_IsShutdown = false;
            AddDefaultEntityGroup();

            foreach (var item in m_EntityGroups)
            {
                Debug.Log(item.Key + "   " + item.Value.GruphId);
            }
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            while (m_RecycleQueue.Count > 0)
            {
                EntityInfo entityInfo = m_RecycleQueue.Dequeue();
                IEntity entity = entityInfo.Entity;
                EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
                if (entityGroup == null)
                {
                    throw new Exception("Entity group is invalid.");
                }

                entityInfo.Status = EntityStatus.WillRecycle;
                entity.OnRecycle();
                entityInfo.Status = EntityStatus.Recycled;
                entityGroup.RemoveEntity(entity);
            }

            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                entityGroup.Value.Update(deltaTime, unscaledDeltaTime);
            }
            /*
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                foreach (var groups in m_EntityGroups)
                {
                    foreach (var entity in groups.Value.GetAllEntitys())
                    {
                        Debug.LogError(entity.Id);
                    }
                }
            }*/
        }

        public void OnLateUpdate()
        {
            
        }

        public void OnFixedUpdate()
        {
            
        }

        public void OnTermination()
        {
            
        }

        private EntityInfo GetEntityInfo(int entityId)
        {
            EntityInfo entityInfo = null;

            if (m_EntityInfos.TryGetValue(entityId, out entityInfo))
            {
                return entityInfo;
            }

            return null;
        }

        // 这个是用于在运行时添加EntityGrouph的 需要同时往EntityGrouph列表里添加
        public bool CreateEntityGroup(string entityGroupName, int groupId)
        {
            foreach (var info in entityGrouphInfos)
            {
                if (info.GroupName == entityGroupName || info.GroupId == groupId)
                {
                    return false;
                }
            }

            if (m_EntityGroups.ContainsKey(entityGroupName))
            {
                return false;
            }
            else
            {
                EntityGroupInfo newInfo = new EntityGroupInfo();
                newInfo.GroupName = entityGroupName;
                newInfo.GroupId = groupId;
                entityGrouphInfos.Add(newInfo);

                EntityGroup entityGroup = new EntityGroup(entityGroupName, groupId);
                m_EntityGroups.Add(entityGroupName, entityGroup);
            }

            return false;
        }

        // 这个是用于添加已经有创建信息的
        public bool AddEntityGroup(string entityGroupName, int groupId)
        {
            if (m_EntityGroups.ContainsKey(entityGroupName))
            {
                return false;
            }
            else
            {
                EntityGroup entityGroup = new EntityGroup(entityGroupName, groupId);
                m_EntityGroups.Add(entityGroupName, entityGroup);
            }
            
            return false;
        }

        /// <summary>
        /// 创建一个默认的实体组
        /// 该组只用于处理特殊的实体
        /// </summary>
        private void AddDefaultEntityGroup()
        {
            //AddEntityGroup("Default", 0);
            foreach (var info in entityGrouphInfos)
            {
                AddEntityGroup(info.GroupName, info.GroupId);
            }
        }

        public IEntityGroup[] GetAllEntityGroups()
        {
            int index = 0;
            IEntityGroup[] results = new IEntityGroup[m_EntityGroups.Count];
            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                results[index++] = entityGroup.Value;
            }
            return results;
        }

        public void GetAllEntityGroups(List<IEntityGroup> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException("Results is invalid.");
            }

            results.Clear();

            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                results.Add(entityGroup.Value);
            }
        }

        public bool HasEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new ArgumentNullException("EntityGroupName is invalid.");
            }

            return m_EntityGroups.ContainsKey(entityGroupName);
        }

        public IEntityGroup GetEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new ArgumentNullException("EntityGroupName is invalid.");
            }

            EntityGroup entityGroup = null;
            if (m_EntityGroups.TryGetValue(entityGroupName,out entityGroup))
            {
                return entityGroup;
            }
            return null;
        }

        public IEntity[] GetAllLoadedEntities()
        {
            int index = 0;
            IEntity[] results = new IEntity[m_EntityInfos.Count];
            foreach (var entity in m_EntityInfos)
            {
                results[index++] = entity.Value.Entity;
            }
            
            return results;
        }

        public void GetAllLoadedEntities(List<IEntity> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException("Results is invalid.");
            }

            results.Clear();

            foreach (KeyValuePair<int, EntityInfo> entity in m_EntityInfos)
            {
                results.Add(entity.Value.Entity);
            }
        }

        public int[] GetAllLoadingEntityIds()
        {
            int index = 0;
            int[] results = new int[m_EntitiesBeingLoaded.Count];
            foreach (var entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results[index++] = entityBeingLoaded.Value;
            }
            return results;
        }

        public void GetAllLoadingEntityIds(List<int> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException("Results is invalid.");
            }

            results.Clear();

            foreach (var entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results.Add(entityBeingLoaded.Value);
            }
        }

        public IEntity GetEntity(int entityId)
        {
            var entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                return null;
            }
            return entityInfo.Entity;
        }

        public bool HasEntity(int entityId)
        {
            return m_EntityInfos.ContainsKey(entityId);
        }

        public void HideAllLoadedEntities()
        {
            HideAllLoadedEntities(null);
        }

        public void HideAllLoadedEntities(object userData)
        {
            while (m_EntityInfos.Count > 0)
            {
                foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
                {
                    InternalHideEntity(entityInfo.Value, userData);
                    break;
                }
            }
        }

        public void HideEntity(int entityId)
        {
            HideEntity(entityId, null);
        }

        public void HideEntity(int entityId, object userData)
        {
            if (IsLoadingEntity(entityId))
            {
                m_EntitiesBeingLoaded.Remove(entityId);
                return;
            }

            EntityInfo entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                throw new Exception(string.Format("Can not find entity '{0}'.", entityId));
            }

            InternalHideEntity(entityInfo, userData);
        }

        public void HideEntity(IEntity entity)
        {
            HideEntity(entity, null);
        }

        public void HideEntity(IEntity entity, object userData)
        {
            if (entity == null)
            {
                throw new Exception("Entity is invalid.");
            }

            HideEntity(entity.Id, userData);
        }

        public bool IsLoadingEntity(int entityId)
        {
            return m_EntitiesBeingLoaded.ContainsKey(entityId);
        }

        public bool IsValidEntity(IEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            return HasEntity(entity.Id);
        }

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName)
        {
            ShowEntity(entityId, entityAssetName, entityGroupName, null);
        }

        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, object userData)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new Exception("Entity asset name is invalid.");
            }

            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new Exception("Entity group name is invalid.");
            }

            if (HasEntity(entityId))
            {
                throw new Exception(string.Format("Entity id '{0}' is already exist.", entityId));
            }

            if (IsLoadingEntity(entityId))
            {
                throw new Exception(string.Format("Entity '{0}' is already being loaded.", entityId));
            }

            EntityGroup entityGroup = (EntityGroup)GetEntityGroup(entityGroupName);
            if (entityGroup == null)
            {
                throw new Exception(string.Format("Entity group '{0}' is not exist.", entityGroupName));
            }


            InternalShowEntity(entityId, entityAssetName, entityGroup, userData);
        }

        private void InternalShowEntity(int entityId, string entityAssetName, EntityGroup entityGroup, object userData)
        {
            try
            {
                IEntity entity = new Entity();
                if (entity == null)
                {
                    throw new Exception("Can not create entity in entity helper.");
                }

                EntityInfo entityInfo = EntityInfo.Create(entity);
                m_EntityInfos.Add(entityId, entityInfo);
                entityInfo.Status = EntityStatus.WillInit;
                entity.OnInit(entityId, entityAssetName, entityGroup, userData);
                entityInfo.Status = EntityStatus.Inited;
                entityGroup.AddEntity(entity);
                entityInfo.Status = EntityStatus.WillShow;
                entity.OnShow(userData);
                entityInfo.Status = EntityStatus.Showed;
            }
            catch
            {
                throw;
            }
        }

        public int GenerateEntityId(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
                throw new Exception("Entity group name is invalid.");

            var entityGroup = (EntityGroup)GetEntityGroup(entityGroupName);

            if (entityGroup == null)
                throw new Exception($"Entity group '{entityGroupName}' is not exist.");

            int entityId = entityGroup.GruphId + entityGroup.EntityCount + 1;

            if (HasEntity(entityId))
                throw new Exception($"Entity id '{entityId}' is already exist.");

            if (IsLoadingEntity(entityId))
                throw new Exception($"Entity '{entityId}' is already being loaded.");

            return entityId;
        }

        public void RegistEntity(IEntity entity, string entityAssetName, string entityGroupName, object userData)
        {
            if (entity == null)
                throw new Exception("Entity is invalid.");

            if (string.IsNullOrEmpty(entityAssetName))
                throw new Exception("Entity asset name is invalid.");

            if (string.IsNullOrEmpty(entityGroupName))
                throw new Exception("Entity group name is invalid.");

            var entityGroup = (EntityGroup)GetEntityGroup(entityGroupName);

            if (entityGroup == null)
                throw new Exception($"Entity group '{entityGroupName}' is not exist.");

            int entityId = GenerateEntityId(entityGroupName);

            InternalRegistEntity(entity, entityId, entityAssetName, entityGroup, userData);
        }

        public void RegistEntity(
            IEntity entity,
            int entityId,
            string entityAssetName,
            string entityGroupName,
            object userData)
        {
            if (entity == null)
                throw new Exception("Entity is invalid.");

            if (string.IsNullOrEmpty(entityAssetName))
                throw new Exception("Entity asset name is invalid.");

            if (string.IsNullOrEmpty(entityGroupName))
                throw new Exception("Entity group name is invalid.");

            var entityGroup = (EntityGroup)GetEntityGroup(entityGroupName);

            if (entityGroup == null)
                throw new Exception($"Entity group '{entityGroupName}' is not exist.");

            if (HasEntity(entityId))
                throw new Exception($"Entity id '{entityId}' is already exist.");

            if (IsLoadingEntity(entityId))
                throw new Exception($"Entity '{entityId}' is already being loaded.");

            InternalRegistEntity(entity, entityId, entityAssetName, entityGroup, userData);
        }

        private void InternalRegistEntity(IEntity entity, int entityId, string entityAssetName, EntityGroup entityGroup, object userData)
        {
            try
            {
                if (entity == null)
                {
                    throw new Exception("Can not create entity in entity helper.");
                }

                EntityInfo entityInfo = EntityInfo.Create(entity);
                m_EntityInfos.Add(entityId, entityInfo);
                entityInfo.Status = EntityStatus.WillInit;
                entity.OnInit(entityId, entityAssetName, entityGroup, userData);
                entityInfo.Status = EntityStatus.Inited;
                entityGroup.AddEntity(entity);
                entityInfo.Status = EntityStatus.WillShow;
                entity.OnShow(userData);
                entityInfo.Status = EntityStatus.Showed;

                Debug.Log(string.Format("成功创建Entity <color=yellow>{0}</color>", entity.Id));
            }
            catch
            {
                throw;
            }
        }

        private void InternalHideEntity(EntityInfo entityInfo, object userData)
        {
            if (entityInfo.Status == EntityStatus.Hidden)
            {
                return;
            }

            IEntity entity = entityInfo.Entity;
            entityInfo.Status = EntityStatus.WillHide;
            entity.OnHide(m_IsShutdown, userData);
            entityInfo.Status = EntityStatus.Hidden;

            EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
            if (entityGroup == null)
            {
                throw new Exception("Entity group is invalid.");
            }

            entityGroup.RemoveEntity(entity);
            if (!m_EntityInfos.Remove(entity.Id))
            {
                throw new Exception("Entity info is unmanaged.");
            }

            m_RecycleQueue.Enqueue(entityInfo);
        }

        public void HideAllLoadingEntities()
        {
            throw new NotImplementedException();
        }
    }
}
