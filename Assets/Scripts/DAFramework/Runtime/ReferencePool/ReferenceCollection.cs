using System;
using System.Collections.Generic;

namespace DAFramework.ReferencePool
{
    public class ReferenceCollection
    {
        /// <summary>
        /// 这个队里里储存里我们目前所有可以使用的对象
        /// </summary>
        private readonly Queue<IReference> m_References;
        private readonly Type m_ReferenceType;
        private int m_UsingReferenceCount;
        private int m_AcquireReferenceCount;
        private int m_ReleaseReferenceCount;
        private int m_AddReferenceCount;
        private int m_RemoveReferenceCount;

        public ReferenceCollection(Type referenceType)
        {
            m_References = new Queue<IReference>();
            m_ReferenceType = referenceType;
            m_UsingReferenceCount = 0;
            m_AcquireReferenceCount = 0;
            m_ReleaseReferenceCount = 0;
            m_AddReferenceCount = 0;
            m_RemoveReferenceCount = 0;
        }

        #region 获取参数的属性
        public Type ReferenceType
        {
            get
            {
                return m_ReferenceType;
            }
        }

        public int UnusedReferenceCount
        {
            get
            {
                return m_References.Count;
            }
        }

        public int UsingReferenceCount
        {
            get
            {
                return m_UsingReferenceCount;
            }
        }

        public int AcquireReferenceCount
        {
            get
            {
                return m_AcquireReferenceCount;
            }
        }

        public int ReleaseReferenceCount
        {
            get
            {
                return m_ReleaseReferenceCount;
            }
        }

        public int AddReferenceCount
        {
            get
            {
                return m_AddReferenceCount;
            }
        }

        public int RemoveReferenceCount
        {
            get
            {
                return m_RemoveReferenceCount;
            }
        }
        #endregion


        /// <summary>
        /// 获取指定类型的引用，核心就是，如果引用队列内有空引用，那么直接获取，如果没有，那么new一个。
        /// </summary>
        /// <typeparam name="T">T必须是继承于IReference的类</typeparam>
        /// <returns></returns>
        public T Acquire<T>() where T : class, IReference, new()
        {
            if (typeof(T) != m_ReferenceType)
            {
                throw new Exception("Type is invalid");
            }

            // 更新 正在使用引用数量
            m_UsingReferenceCount++;
            // 更新 获取引用数量
            m_AcquireReferenceCount++;

            lock (m_References) 
            {
                if (m_References.Count > 0)
                {
                    return m_References.Dequeue() as T;
                }
            }

            // 更新 增加引用数量 
            m_AddReferenceCount++;
            return new T();
        }

        public IReference Acquire()
        {
            // 更新 正在使用引用数量
            m_UsingReferenceCount++;
            // 更新 获取引用数量
            m_AcquireReferenceCount++;

            lock(m_References) 
            {
                if (m_References.Count > 0)
                {
                    return m_References.Dequeue();
                }
            }

            // 更新 增加引用数量
            m_AddReferenceCount++;
            return (IReference)Activator.CreateInstance(m_ReferenceType);
        }

        /// <summary>
        /// 回收引用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reference"></param>
        public void Release(IReference reference)
        {
            // 会在这里调用引用对象的Clear方法，清空数据
            reference.Clear();

            lock (m_References) 
            {
                m_References.Enqueue(reference);
            }

            // 更新 移除引用数量
            m_ReleaseReferenceCount++;
            // 更新 正在使用引用数量
            m_UsingReferenceCount--;
        }

        /// <summary>
        /// 添加指定类型且指定数量的引用。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        /// <exception cref="GameFrameworkException"></exception>
        public void Add<T>(int count) where T : class, IReference, new()
        {
            if (typeof(T) != m_ReferenceType)
            {
                throw new Exception("Type is invalid.");
            }

            lock (m_References)
            {
                m_AddReferenceCount += count;
                while (count-- > 0)
                {
                    m_References.Enqueue(new T());
                }
            }
        }

        public void Add(int count)
        {
            lock (m_References)
            {
                m_AddReferenceCount += count;
                while (count-- > 0)
                {
                    m_References.Enqueue((IReference)Activator.CreateInstance(m_ReferenceType));
                }
            }
        }


        // 移除指定数量的引用
        public void Remove(int count)
        {
            lock (m_References)
            {
                if (count > m_References.Count)
                {
                    count = m_References.Count;
                }

                m_RemoveReferenceCount += count;
                while (count-- > 0)
                {
                    m_References.Dequeue();
                }
            }
        }

        /// <summary>
        /// 删除所有引用
        /// </summary>
        public void RemoveAll()
        {
            lock (m_References)
            {
                m_RemoveReferenceCount += m_References.Count;
                m_References.Clear();
            }
        }
    }
}
