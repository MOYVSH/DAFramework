using UnityEngine;
using Events;

namespace DAFramework.Entity
{
    [RequireComponent(typeof(Entity))]
    public class EntityEventBusComp : EntityComponent
    {
        public BaseEventHandler EvtHandler;

        private void Awake()
        {
            if (EvtHandler == null)
            {
                EvtHandler = new BaseEventHandler(); // 强耦合类可以提前初始化（其实是为了兼容一些狗屎代码）
            }
        }

        public void CreateHandler()
        {
            if (EvtHandler == null)
            {
                EvtHandler = new BaseEventHandler(); // 手动初始化防止发生意外
            }
        }

        protected internal override void OnInit(object userData, Entity owner)
        {
            base.OnInit(userData, owner);
        }

        protected internal override void OnRecycle()
        {
            base.OnRecycle();
            EvtHandler.Clear(); // 解除全部订阅
        }
    }
}

