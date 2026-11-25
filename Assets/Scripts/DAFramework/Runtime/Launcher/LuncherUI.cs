using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LuncherUI : MonoBehaviour
{
    public GameLauncher gameLauncher;
    public Button ChangeScene;
    public Slider LoadProgress;
    public ShaderVariantCollection shaderVariants;

    private string TARGET_SCENE_KEY = "TargetScenePath";

    void Start()
    {
        DebugLogManager.Instance.HideLogWindow();
        ChangeScene.onClick.AddListener(() => 
        {
            // 加载下一个场景，假设场景是顺序加载的
            LoadScene();
        }
        );
        LoadProgress.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
#if UNITY_EDITOR
        // 检查是否有目标场景需要加载
        if (EditorPrefs.HasKey(TARGET_SCENE_KEY))
        {
            string targetScenePath = EditorPrefs.GetString(TARGET_SCENE_KEY);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(targetScenePath);
            LauncherParams launcherParams = new LauncherParams();
            launcherParams.SceneName = sceneName;
            LoadScene(launcherParams);
            EditorPrefs.DeleteKey(TARGET_SCENE_KEY);
            return;
        }
        else
        {
            LoadScene();
        }
#endif
    }
    private async void LoadScene(LauncherParams param = null)
    {
        ChangeScene.gameObject.SetActive(false);
        LoadProgress.gameObject.SetActive(true);
        //StartCoroutine(WarmUpShaders(shaderVariants));
        await UniTask.DelayFrame(1);
        var progress = Cysharp.Threading.Tasks.Progress.Create<float>(
                (p) =>
                {
                    Debug.Log(p);
                    LoadProgress.value = p;
                });
        await gameLauncher.InitModule(progress, param);
        //await gameLauncher.InitModule(progress);
        //await SceneManager.LoadSceneAsync(currentSceneIndex + 1).ToUniTask(progress);
    }

    IEnumerator WarmUpShaders(ShaderVariantCollection svc, int variantsPerFrame = 10)
    {
        while (!svc.WarmUpProgressively(variantsPerFrame))
            yield return null; // 等下一帧继续
        Debug.Log("SVC warmup complete");

    }
}
