using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.Buff
{
    public enum SpeedType
    {
        up,
        dowm
    }

    [CreateAssetMenu(fileName = "TestEffectData Config", menuName = "DAConfig/Buff/TestEffectData Config", order = 4)]
    public class TestEffect : EffectData
    {
        public SpeedType Type;

        [Range(0,10)]
        public float percent;

        public override void InitData(int entityId)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateData(float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        public override void ResetData()
        {
            throw new System.NotImplementedException();
        }
    }
}