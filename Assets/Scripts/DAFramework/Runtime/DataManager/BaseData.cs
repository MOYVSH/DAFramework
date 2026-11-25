using System;
using UnityEngine;
using DAFramework;
using DAFramework.Buff;
using DAFramework.Entity;
using DAFramework.Procedure;

[Serializable]
public class BaseData :MVVM.Core.Model, IData, IAddableBuff
{
    protected Entity _entity;
    public Entity Entity => _entity;
    public int EntityID => _entity.Id;

    protected EntityEventBusComp _eventBus;

    public virtual void InitData(Entity entity)
    {
        _entity = entity;
        if(!TryGetEventBus()) return;
        RegisterEventListeners();
    }

    public virtual void UnInitData()
    {
        UnRegisterEventListeners();
    }

    public virtual void RegisterEventListeners(){}
    public virtual void UnRegisterEventListeners(){}

    protected bool TryGetEventBus()
    {
        if (_entity == null)
        {
            Debug.LogError($"WeaponSystem: Entity with ID {EntityID} not found.");
            return false;
        }

        var ec = EntityHelper.FastGetEntityComponent<EntityEventBusComp>(_entity);
        if (ec == null)
        {
            Debug.LogError($"WeaponSystem: Entity with ID {EntityID} does not have an EntityEventBusComp.");
            return false;
        }

        _eventBus = ec as EntityEventBusComp;
        if (_eventBus == null)
        {
            Debug.LogError($"WeaponSystem: Entity with ID {EntityID} has an invalid EntityEventBusComp.");
            return false;
        }

        if (_eventBus.EvtHandler == null)
        {
            Debug.LogError($"WeaponSystem: Entity with ID {EntityID} has no EvtHandler on its EntityEventBusComp.");
            return false;
        }

        return true;
    }
}
