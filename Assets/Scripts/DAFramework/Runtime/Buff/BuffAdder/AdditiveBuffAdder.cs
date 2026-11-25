namespace DAFramework.Buff
{
    public class AdditiveBuffAdder : IBuffAdder
    {
        public override void OnCheckAdd(BuffBase buff)
        {
            var buffManager = DA.Buff;
            var have = buffManager.TryGetBuff(buff.Owner.EntityID, buff.BuffConfig.m_Buff_Id, out var curBuff);
            if (have == false)
            {
                buffManager.AddNewBuff(buff);
            }
            else
            {
                buffManager.AdditiveBuff(curBuff, buff);
            }
        }
    }
}