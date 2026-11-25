using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DAFramework.Buff;
public class TestBuff : BuffBase
{
    public override void OnAdd()
    {
        base.OnAdd();
        Debug.LogError(this.GetType() + " OnAdd" + " " + Owner.EntityID + " " + BuffConfig.m_Buff_Name);
    }

    public override void OnRemove()
    {
        base.OnRemove();
        Debug.LogError(this.GetType() + " OnRemove" + " " + Owner.EntityID + " " + BuffConfig.m_Buff_Name);
    }

    public override void OnAddCount()
    {
        base.OnAddCount();
        Debug.LogError(this.GetType() + " OnAddCount" + " " + Owner.EntityID + " " + BuffConfig.m_Buff_Name + " " +
                       Count + " " + Progress);
    }

    public override void OnResetTime()
    {
        base.OnResetTime();
        Debug.LogError(this.GetType() + " OnResetTime" + " " + Owner.EntityID + " " + BuffConfig.m_Buff_Name + " " +
                       ActiveTime + " " + Progress);
    }
}