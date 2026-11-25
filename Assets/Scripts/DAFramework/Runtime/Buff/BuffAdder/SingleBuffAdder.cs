using UnityEngine;

namespace DAFramework.Buff
{
    public class SingleBuffAdder : IBuffAdder
    {
        public override void OnCheckAdd(BuffBase buff)
        {
            var buffManager = DA.Buff;
            var have = buffManager.TryGetBuff(buff.Owner.EntityID, buff.BuffConfig.m_Buff_Id, out var buffs);
            if (have == false)
            {
                buffManager.AddNewBuff(buff);
            }
            else
            {
                Debug.LogError("已经存在该Buff,无法重复添加!");
            }
        }
    }
}