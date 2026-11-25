using DACore.Data.Player;
using DAFramework;
using DAFramework.Entity;
using DAFramework.Procedure;
using UnityEngine;

public class LevelNode : ProcedureNode
{
    private LevelDataManager dataManager;

    public override void OnInit(ProcedureProcessor processor)
    {
        Debug.Log("Level0Node OnInit");
        dataManager = new LevelDataManager(0);
    }

    public override void OnEnter(ProcedureProcessor processor)
    {
        Debug.Log("Level0Node OnEnter");
        ModuleCenter.CreateModule<AreaManager>();
        ModuleCenter.CreateModule<PlayerFactory>();
        ModuleCenter.CreateModule<EnemyFactory>();
        SpawnPlayer();
    }

    public override void OnUpdate(ProcedureProcessor processor , float deltaTime, float unscaledDeltaTime)
    {
        //Debug.Log("Level0Node OnUpdate");

        // Buff测试
        // if (Input.GetKeyDown(KeyCode.Z))
        // {
        //     var data = GetNodeData<PlayerOperatorData>(DA.GM.PlayerEntity.Id);
        //     Debug.Log(data.Entity.GetGameObject().name);

        //     //DA.Buff.AddBuff(data,1001);
        //     //DA.Buff.AddBuff(data,1002);
        //     //DA.Buff.AddBuff(data,1003);
        //     DA.Buff.AddBuff(data,1004);
        // }
        
        // if (Input.GetKeyDown(KeyCode.X))
        // {
        //     var data = GetNodeData<PlayerOperatorData>(DA.GM.PlayerEntity.Id);
        //     Debug.Log(data.Entity.GetGameObject().name);
        //     var buffs = DA.Buff.GetEntityBuffs(DA.GM.PlayerEntity.Id);
        //     foreach (var buff in buffs)
        //     {
        //         Debug.LogError(buff.BuffConfig.m_Buff_Name);
        //     }
        // }
    }

    public override void OnExit(ProcedureProcessor processor)
    {
        Debug.Log("Level0Node OnExit");
        ModuleCenter.DestroyModule<AreaManager>();
        ModuleCenter.DestroyModule<PlayerFactory>();
        ModuleCenter.DestroyModule<EnemyFactory>();
    }

    public override void OnDestroy(ProcedureProcessor processor)
    {
        Debug.Log("Level0Node OnDestroy");
    }

    public T GetNodeData<T>(int entityID) where T : BaseData
    {
        return dataManager.GetData<T>(entityID);
    }

    public PlayerData GetPlayerData(int entityID)
    {
        return dataManager.GetData<PlayerData>(entityID);
    }

    public void AddNodeData(Entity entity, BaseData data)
    {
        dataManager.AddData(entity.Id, data);
    }
    
    public void RemoveNodeData(Entity entity)
    {
        dataManager.RemoveData(entity.Id);
    }

    public void SpawnPlayer()
    {
        var spawner = PlayerSpawner.Instance;

        if (spawner != null && spawner.AutoSpawn)
        {
            spawner.Spawn();
        }
    }
}
