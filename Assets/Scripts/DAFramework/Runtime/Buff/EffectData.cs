using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DAFramework.Buff
{
    /// <summary>
    /// 效果数据
    /// </summary>
    public abstract class EffectData : ScriptableObject
    {
        [Tooltip("名称")]
        public string m_Effect_name;

        [Tooltip("描述")]
        [Header("描述")]
        [TextArea(3, 8)]
        public string m_Effect_description;

        public abstract void InitData(int entityId);
        public abstract void UpdateData(float deltaTime);
        public abstract void ResetData();
    }
}