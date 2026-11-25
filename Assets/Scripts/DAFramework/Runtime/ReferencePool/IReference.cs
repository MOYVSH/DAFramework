using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.ReferencePool
{
    /// <summary>
    /// 引用接口，实现这个接口的类会被引用池管理
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// 归还引用时的清理方法
        /// </summary>
        void Clear();
    }
}

