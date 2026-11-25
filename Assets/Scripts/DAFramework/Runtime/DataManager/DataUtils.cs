using System;
using DAFramework;
using DAFramework.Entity;
using DAFramework.Procedure;
using UnityEngine;

public static class DataUtils
{
    /// <summary>
    /// 获取当前 LevelNode
    /// </summary>
    public static LevelNode GetNode()
    {
        // 查找该场景流程中是否有相关的数据 没有就创建一个 有就获取过来
        ProcedureNode tempNode = DA.Procedure.CurrentProcedureNode;

        if (tempNode == null)
        {
            throw new Exception("当前不在ProcedureNode环境");
        }

        LevelNode node = tempNode as LevelNode;

        if (node == null)
        {
            throw new Exception("当前ProcedureNode不是LevelNode");
        }

        return node;
    }

    /// <summary>
    /// 获取或创建 Data
    /// </summary>
    public static T GetOrCreate<T>(
        Entity entity,
        Func<T> creator
    ) where T : BaseData
    {
        var node = GetNode();

        var data = node.GetNodeData<T>(entity.Id);

        if (data == null)
        {
            data = creator();
            node.AddNodeData(entity, data);
        }

        return data;
    }

    /// <summary>
    /// 获取Data
    /// </summary>
    /// <param name="targetID">
    /// 想要获取到目标Entity ID
    /// </param>
    public static T Get<T>(
        int targetID
    ) where T : BaseData
    {
        var node = GetNode();

        var data = node.GetNodeData<T>(targetID);

        if (data == null)
        {
            Debug.LogError("不存在的Data数据");
        }

        return data;
    }

    /// <summary>
    /// 手动添加 Data
    /// </summary>
    public static void Add(Entity entity, BaseData data)
    {
        var node = GetNode();
        node.AddNodeData(entity, data);
    }

    /// <summary>
    /// 移除 Data
    /// </summary>
    public static void Remove(Entity entity)
    {
        var node = GetNode();
        node.RemoveNodeData(entity);
    }
}
