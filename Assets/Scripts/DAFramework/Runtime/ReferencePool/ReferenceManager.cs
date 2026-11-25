using System;
using System.Collections.Generic;

namespace DAFramework.ReferencePool
{
    public class ReferenceManager : ModuleSingleton<ReferenceManager>, IModule
    {
        //暂时不用
        //清理间隔，每过一段时间，就会清空队列里的引用（还在队列里，说明是空引用），m_temp用于实际计算，想要修改清理间隔可以修改clearInterval。
        private float m_ClearInterval = 60;
        private float m_Temp;


        private Dictionary<Type, ReferenceCollection> m_ReferenceCollections;

        #region  IModule

        public void OnInit(object createParam)
        {
            m_ReferenceCollections = new Dictionary<Type, ReferenceCollection>();
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime){}

        public void OnLateUpdate(){}

        public void OnFixedUpdate(){}

        public void OnTermination(){}

        #endregion
       

        /// <summary>
        /// 获取引用池的数量。
        /// </summary>
        public int Count => m_ReferenceCollections.Count;

        public ReferencePoolInfo[] GetAllReferencePoolInfo()
        { 
            int index = 0;
            ReferencePoolInfo[] results = null;
            lock (m_ReferenceCollections)
            {
                results = new ReferencePoolInfo[m_ReferenceCollections.Count];
                foreach (var collection in m_ReferenceCollections)
                {
                    results[index] = new ReferencePoolInfo(collection.Key, collection.Value.UnusedReferenceCount,
                        collection.Value.UsingReferenceCount, collection.Value.AcquireReferenceCount, collection.Value.ReleaseReferenceCount,
                        collection.Value.AddReferenceCount, collection.Value.RemoveReferenceCount);
                    
                    index++;
                }
            }

            return results;
        }

        /// <summary>
        /// 清除所有引用池。
        /// </summary>
        public void ClearAll()
        {
            lock (m_ReferenceCollections)
            {
                foreach (var collection in m_ReferenceCollections)
                {
                    collection.Value.RemoveAll();
                }
                m_ReferenceCollections.Clear();
            }
        }

        /// <summary>
        /// 从引用池获取引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <returns>引用。</returns>
        public T Acquire<T>() where T : class, IReference, new()
        {
            return GetReferenceCollection(typeof(T)).Acquire<T>();
        }

        public IReference Acquire(Type referenceType)
        {
            return GetReferenceCollection(referenceType).Acquire();
        }

        /// <summary>
        /// 归还引用。
        /// </summary>
        /// <param name="reference">引用</param>
        public void Release(IReference reference)
        {
            if (reference == null)
            {
                throw new Exception("不能归还一个空的引用...");
            }

            Type referenceType = reference.GetType();
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Release(reference);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">追加数量。</param>
        public void Add<T>(int count) where T : class, IReference, new()
        {
            GetReferenceCollection(typeof(T)).Add<T>(count);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">追加数量。</param>
        public void Add(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Add(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">移除数量。</param>
        public void Remove<T>(int count) where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">移除数量。</param>
        public void Remove(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Remove(count);
        }

        /// <summary>
        /// 移除特定池中所有的引用。
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        public void RemoveAll<T>() where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).RemoveAll();
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        public void RemoveAll(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).RemoveAll();
        }

        #region 工具函数
        /// <summary>
        /// 检测 引用的类型的合法性。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        private void InternalCheckReferenceType(Type referenceType)
        {
            if (referenceType == null)
            {
                throw new Exception("Reference type is invalid.");
            }

            if (!referenceType.IsClass || referenceType.IsAbstract)
            {
                throw new Exception("Reference type is not a non-abstract class type.");
            }

            if (!typeof(IReference).IsAssignableFrom(referenceType))
            {
                throw new Exception(string.Format("Reference type '{0}' is invalid.", referenceType.FullName));
            }
        }

        /// <summary>
        /// 获取引用池，实际上获取引用的方法。
        /// </summary>
        /// <param name="fullName">类名。</param>
        /// <returns>引用池。</returns>
        private ReferenceCollection GetReferenceCollection(Type type)
        {
            ReferenceCollection result = null;
            lock (m_ReferenceCollections) 
            {
                if (!m_ReferenceCollections.TryGetValue(type, out result))
                {
                    result = new ReferenceCollection(type);
                    m_ReferenceCollections.Add(type, result);
                }
            }

            return result;
        }
        #endregion
    }
}
