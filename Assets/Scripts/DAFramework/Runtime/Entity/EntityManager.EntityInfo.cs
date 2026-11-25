using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAFramework.Entity
{
    public sealed partial class EntityManager : IEntityManager
    {
        private sealed class EntityInfo
        {
            private IEntity m_Entity;
            private EntityStatus m_Status;

            public EntityInfo()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
            }

            public EntityStatus Status
            {
                get
                {
                    return m_Status;
                }
                set
                {
                    m_Status = value;
                }
            }

            public IEntity Entity => m_Entity;

            public static EntityInfo Create(IEntity entity)
            {
                if (entity == null)
                {
                    throw new Exception("Entity is invalid.");
                }

                EntityInfo entityInfo = new EntityInfo();
                entityInfo.m_Entity = entity;
                entityInfo.m_Status = EntityStatus.WillInit;
                return entityInfo;
            }

            public void Clear()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
            }
        }
    }
}
