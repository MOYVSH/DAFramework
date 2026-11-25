using System;
using System.Collections.Generic;

namespace DAFramework.Entity
{
    [Serializable]
    public class EntityGroupInfo
    {
        public string GroupName;
        public int GroupId;
    }


    public class EntityGroup : IEntityGroup
    {
        private string m_name;
        private int m_priority = 1;
        private int m_gruphId = 50000; // default
        private readonly DAFrameworkLinkedList<IEntity> m_Entities;
        private LinkedListNode<IEntity> m_CachedNode;
        

        public string Name => m_name;
        public int Priority => m_priority;
        public int GruphId => m_gruphId;

        public int EntityCount => m_Entities.Count;

        public EntityGroup(string name, int groupId)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Entity group name is invalid.");

            if ((groupId >= 50000 && (groupId % 1000) == 0) || groupId == 0)
            {
                m_gruphId = groupId;
            }
            else
            {
                throw new Exception("Entity group id is invalid.");
            }

            m_name = name;
            m_Entities = new DAFrameworkLinkedList<IEntity>();
            m_CachedNode = null;
        }

        public void GetAllEntities(List<IEntity> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException("Results is invalid.");
            }

            results.Clear();
            
            foreach (var entity in m_Entities)
            {
                results.Add(entity);
            }
        }

        public IEntity[] GetAllEntitys()
        {
            List<IEntity> results = new List<IEntity>();

            foreach (IEntity entity in m_Entities)
            {
                results.Add(entity);
            }

            return results.ToArray();
        }

        public bool HasEntity(int EntityID)
        {
            foreach (var entity in m_Entities)
            {
                if (entity.Id == EntityID)
                {
                    return true;
                }
            }

            return false;
        }

        public IEntity GetEntity(int EntityID)
        {
            foreach (var entity in m_Entities)
            {
                if (entity.Id == EntityID)
                {
                    return entity;
                }
            }

            return null;
        }

        public void AddEntity(IEntity entity)
        {
            m_Entities.AddLast(entity);
        }

        public void RemoveEntity(int EntityID)
        {
            if (HasEntity(EntityID))
            {
                var eneity = GetEntity(EntityID);
                RemoveEntity(eneity);
            }
            else
            {
                throw new Exception(string.Format("Entity group '{0}' not exists specified entity '[{1}]'.", m_name, EntityID));
            }
        }

        public void RemoveEntity(IEntity entity)
        {
            if (m_CachedNode != null && m_CachedNode.Value == entity)
            {
                m_CachedNode = m_CachedNode.Next;
            }
            if (!m_Entities.Remove(entity))
            {
                throw new Exception(string.Format("Entity group '{0}' not exists specified entity '[{1}]'.", m_name, entity.Id));
            }
        }

        /// <summary>
        /// 实体组轮询。
        /// </summary>
        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            LinkedListNode<IEntity> current = m_Entities.First;
            while (current != null)
            {
                m_CachedNode = current.Next;
                current.Value.OnUpdate(deltaTime, unscaledDeltaTime);
                current = m_CachedNode;
                m_CachedNode = null;
            }
        }
    }
}
