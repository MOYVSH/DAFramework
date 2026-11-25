using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DAPool;
using DAFramework;
using DAFramework.Buff;
using DAFramework.Entity;
using DAFramework.Procedure;
using DAFramework.ReferencePool;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///  GameLauncher：框架启动入口，负责初始化模块中心并驱动游戏主流程
/// </summary>

public class GameLauncher : MonoBehaviour
{
    private float _progress = 0;
    public async UniTask InitModule(System.IProgress<float> progress, LauncherParams param = null)
    {
        // 记录初始化开始时间的秒
        long startTimeInSeconds = DateTime.UtcNow.GeUtcToSeconds();
        Debug.Log($"Initialization started at: {startTimeInSeconds} seconds");
        
        // 初始化模块中心
        ModuleCenter.Initialize(this);
        await UniTask.DelayFrame(3);
        _progress += 0.1f;
        progress.Report(_progress);

        DA.Reference = await CreateModel<ReferenceManager>(progress);
        DA.Config = await CreateModel<ConfigManager>(progress);
        DA.Entity = await CreateModel<EntityManager>(progress);
        DA.UI = await CreateModel<UIManager>(progress);
        DA.Buff = await CreateModel<BuffManager>(progress);
        DA.Procedure = await CreateModel<ProcedureManager>(progress);
        DA.GM = await CreateModel<GameManager>(progress);
        DA.Pool = await CreateModel<ObjectPoolManager>(progress);

        await GetConfigs(progress);
        await StartGame(progress, param);
        
        long finishTimeInSeconds = DateTime.UtcNow.GeUtcToSeconds();
        Debug.Log($"Initialization finished at: {finishTimeInSeconds} seconds");
        DateTimeExtensions.GameStarTime = finishTimeInSeconds;
    }

    public async UniTask<T> CreateModel<T>(System.IProgress<float> progress) where T : class, IModule
    {
        var model = ModuleCenter.CreateModule<T>();
        await UniTask.DelayFrame(3);

        _progress += 0.1f;
        progress.Report(_progress);
        return model;
    }

    public async UniTask GetConfigs(System.IProgress<float> progress)
    {
        await SOProvider.InitAllConfig();
        
        _progress += 0.1f;
        progress.Report(_progress);
    }

    // 开始游戏
    public async UniTask StartGame(System.IProgress<float> progress, LauncherParams param = null)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 测试功能 进入第0关的时候 创建流程
        DA.Procedure.AddProcedureNodes(new LevelNode());
        if (param != null)
        {
            await SceneManager.LoadSceneAsync(param.SceneName).ToUniTask(progress);
        }
        else
        {
            await SceneManager.LoadSceneAsync(currentSceneIndex + 1).ToUniTask(progress);
            // 进入场景后 启动节点
            DA.Procedure.RunProcedureNode<LevelNode>();
        }
    }

    public async UniTask ChangeScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        await SceneManager.LoadSceneAsync(currentSceneIndex + 1).ToUniTask();
    }
    
    void Update()
    {
        // 更新模块
        ModuleCenter.Update();
    }
    
    void LateUpdate()
    {
        // 更新模块
        ModuleCenter.LateUpdate();
    }
    
    void FixedUpdate()
    {
        // 更新模块
        ModuleCenter.FixedUpdate();
    }
}
