using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework.Buff
{
    [CreateAssetMenu(fileName = "TestBuffData Config", menuName = "DAConfig/Buff/TestBuffData Config", order = 3)]
    public class TestBuffConfig : BuffConfigData
    {
        [Tooltip("自定义属性1")]
        [Header("自定义属性1")]
        public int custom1;

        [Tooltip("自定义属性2")]
        [Header("自定义属性2")]
        public string custom2;
    }
}