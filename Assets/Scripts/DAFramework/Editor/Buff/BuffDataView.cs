using DAFramework.Buff;
using System;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class BuffDataView : VisualElement
{
    private VisualElement dataView;

    private BuffConfigData currentBuffConfigData;
    private SerializedObject currentSerializedObject;
    private List<SerializedProperty> currentSerializePropertys;

    public BuffDataView()
    {
        var dataViewAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath.BUFF_DATA_VIEW);
        dataView = dataViewAsset.Instantiate();

        Add(dataView);
    }

    internal void SetBuffData(BuffConfigData buffConfigData)
    {
        if (currentBuffConfigData != buffConfigData)
        {
            currentBuffConfigData = buffConfigData;
            currentSerializedObject = new SerializedObject(currentBuffConfigData);
            currentSerializePropertys = currentSerializedObject.GetAllSerializedProperty();
        }
        if (currentSerializedObject == null)
        {
            currentSerializedObject = new SerializedObject(currentBuffConfigData);
            currentSerializePropertys = currentSerializedObject.GetAllSerializedProperty();
        }

        List<SerializedProperty> tempPropertyFields = new List<SerializedProperty>();
        
        int propCount = currentSerializePropertys.Count;
        while (propCount > 0)
        {
            foreach (var property in currentSerializePropertys)
            {
                if (!tempPropertyFields.Contains(property))
                {
                    if (property.name == "m_Buff_Id")
                    {
                        tempPropertyFields.Insert(0, property);
                        propCount -= 1;
                        continue;
                    }
                    if (property.name == "m_Buff_Name")
                    {
                        tempPropertyFields.Insert(1, property);
                        propCount -= 1;
                        continue;
                    }

                    tempPropertyFields.Add(property);
                    propCount -= 1;
                }
                else
                { 
                    continue;
                }
            }
        }

        dataView.Clear();
        for (int i = 0; i < tempPropertyFields.Count; i++)
        {
            SerializedProperty property = tempPropertyFields[i];

            var propertyField = new PropertyField(property);
            propertyField.Bind(currentSerializedObject);
            //propertyField.RegisterValueChangeCallback((evt) => {});
            dataView.Add(propertyField);
        }
    }
}