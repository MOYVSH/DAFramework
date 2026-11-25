using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DAFramework.Buff
{
    [CreateAssetMenu(fileName = "Buffs Config", menuName = "DAConfig/Buff/Buffs Config", order = 1)]
    public class BuffsConfig : ScriptableObject
    {
        [HideInInspector]
        public int start_id;


        //[HideInInspector]
        [Tooltip("Buff配表集合")]
        public List<BuffConfigData> buffs = new List<BuffConfigData>();

        private Dictionary<int, BuffConfigData> buffs_dic = new Dictionary<int, BuffConfigData>();

        /// <summary>
        /// 查询Buff配置信息
        /// </summary>
        /// <param name="buffId"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public BuffConfigData GetBuffData(int buffId)
        {

            if (buffs_dic.Count == 0)
            {
                foreach (var item in buffs)
                {
                    if (buffs_dic.ContainsKey(item.m_Buff_Id))
                        throw new System.Exception(string.Format("BuffInfo id:{0}重复!", item.m_Buff_Id));
                    buffs_dic.Add(item.m_Buff_Id, item);
                }
            }

            if (buffs_dic.ContainsKey(buffId))
                return buffs_dic[buffId];

            return null;
        }

        public void ClearCache()
        {
            buffs_dic.Clear();
        }


#if UNITY_EDITOR

        public void Refresh(MonoScript script)
        {
            string fullName = script.GetClass().FullName;

            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i] == null) continue;
                if (buffs[i].buff_class_full_name == fullName)
                {
                    buffs[i].buff_class_full_name = string.Empty;
                    EditorUtility.SetDirty(buffs[i]);
                }
            }
        }

#endif
    }
}