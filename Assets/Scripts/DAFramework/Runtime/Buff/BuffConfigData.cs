using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DAFramework.Buff
{
    [Serializable]
    public class IconInfo
    {
        public string _projectName;
        public string _iconName;
    }

    public enum BuffMode
    {
        /// <summary>
        /// 不可叠加 具有第一唯一性 不可被重置时间
        /// </summary>
        [Tooltip("不可叠加")]
        Single,
        /// <summary>
        /// 可叠加 在原有的基础上叠加 并重置时间
        /// </summary>
        [Tooltip("可叠加")]
        Additive,
        /// <summary>
        /// 替代 替换掉原本要走的Buff 会重新走OnAdd逻辑 和被替换掉的buff的 OnRemove逻辑
        /// </summary>
        [Tooltip("替代")]
        Replace,
        /// <summary>
        /// 重置 只重置时间
        /// </summary>
        [Tooltip("重置")]
        ResetTime
        
    }

    /// <summary>
    /// Buff配置数据
    /// </summary>
    [CreateAssetMenu(fileName = "BuffData Config", menuName = "DAConfig/Buff/BuffData Config", order = 2)]
    public class BuffConfigData : ScriptableObject
    {
        [Tooltip("唯一标识符")]
        [Header("唯一标识符")]
        public int m_Buff_Id;

        [Tooltip("名称")]
        [Header("名称")]
        public string m_Buff_Name;

        [Tooltip("图标")]
        [Header("图标")]
        public IconInfo m_Buff_Icon;

        [Tooltip("描述")]
        [Header("描述")]
        [TextArea(3, 8)]
        public string m_Buff_Description;

        [Tooltip("持续时间(如果小于等于0,认为该Buff长期存在!)")]
        [Header("持续时间")]
        public float m_Buff_Duration;
        
        [Tooltip("模式(如果有多个相同的buff，它们的效果是否能够叠加?)")]
        [Header("模式")]
        public BuffMode m_Buff_mode = BuffMode.Additive;
        
        [Tooltip("最大叠加次数(如果叠加次数超过最大叠加次数,则不添加Buff)")]
        [Header("最大叠加次数")]
        public int m_Max_count = 1;
        
        [Tooltip("Buff效果(当前Buff有哪些效果)")]
        [Header("Buff效果")]
        public List<EffectData> m_Buff_effects = new List<EffectData>();

        [Tooltip("Buff脚本名称")]
        public string buff_class_full_name;

#if UNITY_EDITOR

        public void Rename()
        {
            string asset_path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.RenameAsset(asset_path, m_Buff_Id.ToString());
        }

#endif
    }
}