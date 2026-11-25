using System;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.Procedure
{
    public sealed class ProcedureProcessor
    {
        // 当前节点
        ProcedureNode currentNode;

        // 存储节点的字典
        Dictionary<Type, ProcedureNode> typeNodeDict = new Dictionary<Type, ProcedureNode>();

        // 节点相关回调
        Action<Type, Type> onProcedureNodeChange;
        Action<Type> onProcedureNodeAdd;
        Action<Type> onProcedureNodeRemove;

        /// <summary>
        /// ExitType，EnterType
        /// </summary>
        public event Action<Type, Type> OnProcedureNodeChange
        {
            add { onProcedureNodeChange += value; }
            remove { onProcedureNodeChange -= value; }
        }

        public event Action<Type> OnProcedureNodeAdd
        {
            add { onProcedureNodeAdd += value; }
            remove { onProcedureNodeAdd -= value; }
        }

        public event Action<Type> OnProcedureNodeRemove
        {
            add { onProcedureNodeRemove += value; }
            remove { onProcedureNodeRemove -= value; }
        }

        /// <summary>
        /// 当前状态；
        /// </summary>
        public ProcedureNode CurrentNode
        {
            get { return currentNode; }
        }

        /// <summary>
        /// 状态机持有者；
        /// </summary>
        public ProcedureManager Handle { get; private set; }

        /// <summary>
        /// 状态数量；
        /// </summary>
        public int NodeCount
        {
            get { return typeNodeDict.Count; }
        }

        /// <summary>
        /// 构造函数；
        /// </summary>
        /// <param name="handle">状态机持有者对象</param>
        internal ProcedureProcessor(ProcedureManager handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// 添加一个状态
        /// </summary>
        /// <param name="node">状态</param>
        /// <returns>添加结果</returns>
        public bool AddNode(ProcedureNode node)
        {
            if (node == null)
            {
                return false;
            }

            Type nodeType = node.GetType();
            if (typeNodeDict.ContainsKey(nodeType))
            {
                return false;
            }

            typeNodeDict.Add(nodeType, node);
            node.OnInit(this);
            if (onProcedureNodeAdd != null)
            {
                onProcedureNodeAdd(nodeType);
            }

            return true;
        }

        /// <summary>
        /// 添加一组状态
        /// </summary>
        /// <param name="nodes">状态集合</param>
        public void AddNodes(params ProcedureNode[] nodes)
        {
            var length = nodes.Length;
            for (int i = 0; i < length; i++)
            {
                AddNode(nodes[i]);
            }
        }

        /// <summary>
        /// 移除一个状态
        /// </summary>
        /// <param name="nodeType">状态类型</param>
        /// <returns>移除结果</returns>
        public bool RemoveNode(Type nodeType)
        {
            if (nodeType == null)
            {
                return false;
            }

            if (!typeNodeDict.ContainsKey(nodeType))
            {
                return false;
            }

            typeNodeDict.Remove(nodeType);
            if (onProcedureNodeRemove != null)
            {
                onProcedureNodeRemove(nodeType);
            }

            return true;
        }

        /// <summary>
        /// 是否存在状态
        /// </summary>
        /// <param name="nodeType">状态类型</param>
        /// <returns>存在结果</returns>
        public bool ContentNode(Type nodeType)
        {
            return typeNodeDict.ContainsKey(nodeType);
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <param name="nodeType">状态类型</param>
        /// <param name="node">获取的状态</param>
        /// <returns>获取结果</returns>
        public bool PeekNode(Type nodeType, out ProcedureNode node)
        {
            return typeNodeDict.TryGetValue(nodeType, out node);
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        /// <param name="unscaledDeltaTime">真实帧间隔时间</param>
        public void Refresh(float deltaTime, float unscaledDeltaTime)
        {
            currentNode?.OnUpdate(this, deltaTime, unscaledDeltaTime);
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="nodeType">状态类型</param>
        public void ChangeNode(Type nodeType)
        {
            if (nodeType == null)
            {
                return;
            }

            if (!typeNodeDict.ContainsKey(nodeType))
            {
                return;
            }

            if (typeNodeDict.TryGetValue(nodeType, out var state))
            {
                if (state != null)
                {
                    currentNode?.OnExit(this);
                    var exitedNodeType = currentNode == null ? null : currentNode.GetType();
                    currentNode = state;
                    currentNode?.OnEnter(this);
                    var enteredNodeType = currentNode == null ? null : currentNode.GetType();
                    onProcedureNodeChange?.Invoke(exitedNodeType, enteredNodeType);
                }
            }
        }

        /// <summary>
        /// 清理所有状态
        /// </summary>
        public void ClearAllNode()
        {
            currentNode?.OnExit(this);
            foreach (var state in typeNodeDict.Values)
            {
                state?.OnDestroy(this);
            }

            typeNodeDict.Clear();
        }
    }
}
