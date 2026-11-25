using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace DAFramework.Buff
{
    [UpdateRefresh]
    public class BuffManager : ModuleSingleton<BuffManager>, IModule
    {
        #region 字段

        private Dictionary<int, List<BuffBase>> _entityAndBuffs =
            new Dictionary<int, List<BuffBase>>();

        // 用于添加Buff
        private List<BuffBase> _addBuffList = new List<BuffBase>();
        
        // 用于移除受Buff影响的对象
        private List<int> _removeEntityList = new List<int>();
        // 用于移除Buff
        private List<BuffBase> _removeBuffList = new List<BuffBase>();
        
        // 用于定期移除 Addable 的 Tick
        private float _removeAddableTick = 0;
        
        // 给获取效果的方法使用
        private List<BuffBase> _tempBuffs = new List<BuffBase>();

        // 提前缓存好的Buff添加器
        private Dictionary<BuffMode, IBuffAdder> _buffAdders = new Dictionary<BuffMode, IBuffAdder>();
        #endregion

        #region Config

        private List<BuffsConfig> configs = new List<BuffsConfig>();

        static ProfilerMarker s_PreparePerfMarker1 = new ProfilerMarker("BuffManager.AddBuff1");
        static ProfilerMarker s_PreparePerfMarker2 = new ProfilerMarker("BuffManager.AddBuff2");
        static ProfilerMarker s_PreparePerfMarker3 = new ProfilerMarker("BuffManager.AddBuff3");
        static ProfilerMarker s_PreparePerfMarker4 = new ProfilerMarker("BuffManager.AddBuff4");
        static ProfilerMarker s_PreparePerfMarker5 = new ProfilerMarker("BuffManager.AddBuff5");

        
        /// <summary>
        /// 添加Buff配表
        /// </summary>
        /// <param name="buffConfig">Buff配置</param>
        /// <exception cref="Exception">ID重复</exception>
        private void AddConfig(BuffsConfig buffConfig)
        {
            if (configs.Contains(buffConfig))
            {
                Debug.LogWarningFormat("已添加Buff配置,请勿重复添加!");
                return;
            }

            foreach (var buff in buffConfig.buffs)
            {
                foreach (var config in configs)
                {
                    if (config.GetBuffData(buff.m_Buff_Id) == null)
                        continue;

                    StringBuilder builder = new StringBuilder();
                    builder.Append("检测到Buff id重复,");
                    builder.Append("配置:").Append(buffConfig.name);
                    builder.Append("Buff:").Append(buff.m_Buff_Id).Append(buff.m_Buff_Id);
                    builder.Append("配置:").Append(config.name);
                    builder.Append("Buff:").Append(buff.m_Buff_Id).Append(config.GetBuffData(buff.m_Buff_Id).m_Buff_Name);

                    throw new Exception(builder.ToString());
                }
            }

            configs.Add(buffConfig);
        }

        /// <summary>
        /// 移除Buff配表
        /// </summary>
        /// <param name="buffConfig"></param>
        public void RemoveConfig(BuffsConfig buffConfig)
        {
            if (configs.Contains(buffConfig))
            {
                configs.Remove(buffConfig);
            }
        }
        #endregion
        public void OnInit(object createParam)
        {
            // TODO: 资源热更方案待定——原通过 Addressables 加载 key="BuffConfig" 配表的逻辑已移除。
            // 新资源系统接入后，于此加载 BuffsConfig 并调用 AddConfig(...)。
            // 示例：DA.Res.LoadAssetAsync<BuffsConfig>("BuffConfig", (config) => AddConfig(config.Result));
            
            _buffAdders.Add(BuffMode.Single, new SingleBuffAdder());
            _buffAdders.Add(BuffMode.Additive, new AdditiveBuffAdder());
            _buffAdders.Add(BuffMode.Replace, new ReplaceBuffAdder());
            _buffAdders.Add(BuffMode.ResetTime, new ResetTimeBuffAdder());

        }

        
        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            // 遍历 _removeBuffList 从 _entityAndBuffs 中移除
            RemoveBuffData();
            
            // 遍历 _addBuffList 对需要添加的 Buff 进行添加
            AddBuffData();
            
            // 将 _entityAndBuffs 中没有buff的 addable 置入 _removeEntityList
            BuildRemoveEntityList();
            
            // 遍历 _entityAndBuffs 对需要 Update 的 Buff 进行更新
            // 并将到期的 Buff 添加到 _removeBuffList
            UpdateBuffs(deltaTime, unscaledDeltaTime);
            
            _removeAddableTick += unscaledDeltaTime;
            // 定期移除Addable
            if (_removeAddableTick >= 1)
            {
                _removeAddableTick = 0;
                foreach (var item in _removeEntityList)
                {
                    if (_entityAndBuffs.ContainsKey(item))
                    {
                        _entityAndBuffs.Remove(item);
                    }
                }
                _removeEntityList.Clear();
            }
        }
        
        #region 模块中非必要函数
        public void OnFixedUpdate(){}
        public void OnLateUpdate(){}
        public void OnTermination(){}
        #endregion
        
        #region 方法
        // 查询Buff的配置信息 
        private BuffConfigData GetBuffConfig(int buff_id)
        {
            foreach (var item in configs)
            {
                BuffConfigData data = item.GetBuffData(buff_id);
                if (data != null)
                    return data;
            }

            return null;
        }

        /// <summary>
        /// 添加可添加Buff的对象及其Buff列表
        /// </summary>
        public void AddAddable(int entityId)
        {
            if (!_entityAndBuffs.ContainsKey(entityId))
            {
                _entityAndBuffs.Add(entityId, new List<BuffBase>());
            }
        }
        
        /// <summary>
        /// 移除可添加Buff的对象及其Buff列表
        /// </summary>
        public void RemoveAddable(int entityId)
        {
            if (_entityAndBuffs.ContainsKey(entityId))
            {
                _removeEntityList.Add(entityId);
            }
        }

        /// <summary>
        /// 无视条件直接添加一个buff 条件在其他地方检测
        /// </summary>
        public void AddNewBuff(BuffBase buff)
        {
            if (_addBuffList.ToList().Any(buffBase => buffBase.BuffConfig.m_Buff_Id == buff.BuffConfig.m_Buff_Id))
            {
                return;
            }

            _addBuffList.Add(buff);
        }

        /// <summary>
        /// 替换一个buff 以新换旧
        /// </summary>
        /// <param name="buff"></param>
        public void ReplaceBuff(BuffBase curBuff, BuffBase newBuff)
        {
            // 如果 _removeBuffList 中已经存在则不添加curBuff 否则添加
            if (!_removeBuffList.Contains(curBuff))
            {
                _removeBuffList.Add(curBuff);
            }
            // 如果 _addBuffList 中已经存在则不添加newBuff 否则添加
            if (!_addBuffList.Contains(newBuff))
            {
                _addBuffList.Add(newBuff);
            }
        }
        
        /// <summary>
        /// 给原有的buff叠层 并重置时间
        /// </summary>
        public void AdditiveBuff(BuffBase curBuff, BuffBase newBuff)
        {
            // 如果 _removeBuffList 中已经存在则将其移出
            if (_removeBuffList.Contains(curBuff))
            {
                _removeBuffList.Remove(curBuff);
            }
            
            // 如果 _addBuffList 存在newBuff 则不添加
            if (_addBuffList.Contains(newBuff))
            {
                return;
            }
            
            // 进行叠层
            curBuff.AddCount();
            
            DA.Reference.Release(newBuff);
        }

        public void ResetBuff(BuffBase curBuff, BuffBase newBuff)
        {
            // 如果 _removeBuffList 中已经存在则将其移出
            if (_removeBuffList.Contains(curBuff))
            {
                _removeBuffList.Remove(curBuff);
            }
            
            // 如果 _addBuffList 存在newBuff 则不添加
            if (_addBuffList.Contains(newBuff))
            {
                return;
            }
            
            // 进行重置
            curBuff.ResetTime();
            DA.Reference.Release(newBuff);
        }
        
        private BuffBase tempBuff;
        /// <summary>
        /// 添加Buff
        /// </summary>
        public void AddBuff(IAddableBuff addable, int buffID)
        {
            if (addable == null)
            {
                Debug.LogError("未查询到Buff添加对象");
                return;
            }
            
            int entityId = addable.EntityID;
            
            // 检查 _entityAndBuffs 中是否有 entityId 没有则添加
            if (!_entityAndBuffs.ContainsKey(entityId))
            {
                AddAddable(entityId);
            }
            
            tempBuff = CreateBuffObj(buffID);
            if (tempBuff == null)
            {
                Debug.LogError("Buff创建失败");
                return;
            }
            
            tempBuff.Owner = addable;
            
            if (_entityAndBuffs[entityId].Count == 0)
            {
                _entityAndBuffs[entityId].Add(tempBuff);
                tempBuff.OnAdd();
            }
            else
            {
                // 根据Buff的 BufferAdder 添加Buff
                var buffAdder = _buffAdders[tempBuff.BuffConfig.m_Buff_mode];
                buffAdder.OnCheckAdd(tempBuff);
            }
        }

        /// <summary>
        /// 添加一组Buff
        /// </summary>
        public void AddBuffs(IAddableBuff addable, IList<int> buffs)
        {
            if (addable == null)
            {
                Debug.LogError("未查询到Buff添加对象");
                return;
            }
            
            if (buffs == null || buffs.Count == 0)
            {
                Debug.LogError("无Buff列表");
                return;
            }

            foreach (var item in buffs)
            {
                AddBuff(addable, item);
            }
        }
        
        /// <summary>
        /// 移除Buff
        /// </summary>
        public void RemoveBuff(int entityId, BuffBase buff)
        {
            if (entityId == 0 || buff == null)
                return;

            bool b = _entityAndBuffs.ContainsKey(entityId);
            if (!b) 
                return;
            // 检测 _removeBuffList 中是否已经存在 如果不存在则添加
            if (!_removeBuffList.Contains(buff))
                _removeBuffList.Add(buff);
        }
        
        /// <summary>
        /// 移除一组buff
        /// </summary>
        public void RemoveBuffs(int entityId, IList<BuffBase> buffs)
        {
            if (entityId == 0 || buffs == null) 
                return;

            if (!_entityAndBuffs.ContainsKey(entityId))
                return;
            if (_removeBuffList.Contains(buffs[0]))
                return;
            if (_removeEntityList.Contains(entityId))
                return;

            
            foreach (var item in buffs)
            {
                RemoveBuff(entityId, item);
            }
        }

        /// <summary>
        /// 查询效果
        /// </summary>
        /// <typeparam name="T"> 继承于EffectData的Buff数据配置 </typeparam>
        public void GetEffects<T>(int entityId, ref List<T> results) where T : EffectData
        {
            if (results == null)
            {
                results = new List<T>();
            }
            else
            {
                results.Clear();
            }
            
            var b = _entityAndBuffs.TryGetValue(entityId, out _tempBuffs);
            if (b == false) 
                return;

            foreach( var buff in _tempBuffs)
            {
                foreach (var effect in buff.BuffConfig.m_Buff_effects)
                {
                    if (!(effect is T)) continue;
                    T effectInfo = effect as T;
                    results.Add(effectInfo);
                }
            }
        }

        /// <summary>
        /// 获取Buff
        /// </summary>
        public bool TryGetBuff(int entityId, int buffID, out BuffBase buff)
        {
            buff = null;
            if (!_entityAndBuffs.TryGetValue(entityId, out _tempBuffs))
                return false;

            foreach (var item in _tempBuffs)
            {
                if (item.BuffConfig.m_Buff_Id == buffID)
                {
                    buff = item;
                    return true;
                }
            }

            return false;
        }

        public List<BuffBase> GetEntityBuffs(int entityId)
        {
            var _getBuffs = _entityAndBuffs.GetValueOrDefault(entityId);
            
            return _getBuffs ?? new List<BuffBase>();
        }
        
        private BuffBase CreateBuffObj(int buffID)
        {
            // 获取到类型 
            BuffConfigData buffConfig = GetBuffConfig(buffID);

            if (buffConfig == null)
            {
                Debug.LogError(string.Format("未查询到Buff数据,id:{0}", buffID));
                return null;
            }

            Type type = Type.GetType(buffConfig.buff_class_full_name);
            if (type == null)
                type = typeof(BuffBase);

            BuffBase buff = DA.Reference.Acquire(type) as BuffBase;

            if (buff != null)
            {
                buff.BuffConfig = buffConfig;
                buff.Reset();

                return buff;
            }

            return null;
        }

        /// <summary>
        /// 从 _entityAndBuffs 中移除Buff实例
        /// </summary>
        private void RemoveBuffData()
        {
            foreach (var item in _removeBuffList)
            {
                foreach (var addableAndBuff in _entityAndBuffs)
                {
                    if (addableAndBuff.Value.Contains(item))
                    {
                        item.OnRemove();
                        addableAndBuff.Value.Remove(item);
                        DA.Reference.Release(item);
                    }
                }
            }
            
            _removeBuffList.Clear();
        }
        
        /// <summary>
        /// 添加Buff实例到 _entityAndBuffs 中
        /// </summary>
        private void AddBuffData()
        {
            // 遍历 _addBuffList 对需要添加的 Buff 进行添加
            foreach (var item in _addBuffList)
            {
                // 这个阶段 _removeBuffList 已经被清空 且已经被移除过这些buff了
                // 如果拥有对象在 _removeEntityList 则将其移出 如果没有则添加
                var entityId = item.Owner.EntityID;
                if (_removeEntityList.Contains(entityId))
                {
                    _removeEntityList.Remove(entityId);
                }
                else
                {
                    if (_entityAndBuffs.ContainsKey(entityId))
                    {
                        _entityAndBuffs[entityId].Add(item);
                    }
                    else
                    {
                        return;
                    }
                }
                
                item.OnAdd();
            }
            _addBuffList.Clear();
        }
        
        private void UpdateBuffs(float deltaTime, float unscaledDeltaTime)
        {
            // 遍历 _entityAndBuffs 对需要 Update 的 Buff 进行更新
            // 并将到期的 Buff 添加到 _removeBuffList
            foreach (var item in _entityAndBuffs)
            {
                int entityId = item.Key;
                List<BuffBase> buffs = item.Value;
                // 如果 entityId 在 _removeEntityList 中则跳过
                if (_removeEntityList.Contains(entityId))
                    continue;
                
                foreach (var buff in buffs)
                {
                    // 检测 buff 状态 如果到期则添加到 _removeBuffList
                    if (buff.Progress >= 1)
                    {
                        RemoveBuff(entityId, buff);
                    }
                    
                    // 如果buff 在 _removeBuffList 中则跳过
                    if (_removeBuffList.Contains(buff))
                        continue;

                    buff.Update(deltaTime);
                }
            }
        }
        
        /// <summary>
        /// 根据 _entityAndBuffs 中的 buff 状态 构建移除 Entity 列表
        /// </summary>
        private void BuildRemoveEntityList()
        {
            foreach (var item in _entityAndBuffs)
            {
                if (item.Value.Count == 0)
                {
                    // _removeEntityList 中没有该 Entity 才添加
                    if (!_removeEntityList.Contains(item.Key))
                        _removeEntityList.Add(item.Key);
                }
            }
        }
        
        #endregion
    }
}