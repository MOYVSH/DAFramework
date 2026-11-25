using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DAFramework
{
    /// <summary>
    /// ConfigManager 用于读取类的模块
    /// 暂时使用手动添加的形式 后期考虑搞成自动生成式
    /// </summary>
    public class ConfigManager : ModuleSingleton<ConfigManager>, IModule
    {
        /* 表对应的字段 */
        //private ExcelExample p_ExcelExample;

        public IEnumerable LoadAllAsync()
        {
            yield return null;
            //yield return LoadAsync<ExcelExample>("ExcelExample", result => p_ExcelExample = result as ExcelExample);
        }

        public T Load<T>(string name) where T : UnityEngine.ScriptableObject
        {
            var result = Resources.Load("Export/" + name) as T;
            return result;
        }

        public IEnumerator LoadAsync<T>(string name, Action<T> callback) where T : UnityEngine.ScriptableObject
        {
            var load = Resources.LoadAsync<T>("Export/" + name);
            yield return load;
            T asset = load.asset as T;
            if (asset != null)
            {
                callback(asset);
            }
        }

        /* 表对应的属性 用于沟通外界 */
        //public ExcelExample ExcelExample { get => p_ExcelExample;}



        #region 不太那么重要的函数 特殊情况下酌情使用
        public void OnInit(object createParam)
        {

        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {

        }

        public void OnLateUpdate()
        {

        }

        public void OnFixedUpdate()
        {

        }
        #endregion

        public void OnTermination()
        {
            base.Destroy();
        }
    }
}
