#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;


using UnityEditor;


public static class SerializedObjectExtensions
{
    /// <summary>
    /// 通过反射的方式获取到SerializedObject中所有的可以序列化的字段
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <returns></returns>
    public static List<SerializedProperty> GetAllSerializedProperty(this SerializedObject serializedObject)
    {
        List<SerializedProperty> properties = new List<SerializedProperty>();

        Type type = serializedObject.targetObject.GetType();

        List<FieldInfo> fields = GetFields(type);

        foreach (FieldInfo field in fields)
        {
            if (!field.IsInitOnly) // 忽略只读字段 
            {
                SerializedProperty property = serializedObject.FindProperty(field.Name);
                if (property == null) continue;
                if (IsContainProperty(property, properties)) continue;
                properties.Add(property);
            }
        }

        return properties;
    }

    private static bool IsContainProperty(SerializedProperty property, List<SerializedProperty> properties)
    {
        foreach (var item in properties)
        {
            if (item == null) continue;
            if (item.name == property.name) return true;
        }

        return false;
    }

    private static List<FieldInfo> GetFields(Type type)
    {

        if (type == null) return null;

        List<FieldInfo> fields = new List<FieldInfo>();

        fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
        if (type.BaseType != null)
            fields.AddRange(GetFields(type.BaseType));
        return fields;
    }
}
#endif
