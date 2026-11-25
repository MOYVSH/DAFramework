using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DAFramework.Buff
{
    public class BuffConfigsWindow : EditorWindow
    {
        private Button button;
        private ListView listView;
        private VisualElement buffInfo;

        private BuffDataView buffDataView;

        private List<BuffConfigData> BuffConfigs;
        private int itemHeight = 55;


        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [SerializeField]
        private StyleSheet styleSheet;
        private SerializedObject serializedObject;

        [MenuItem("配置工具/BUFF模板配置界面")]
        public static void Open()
        {
            //// 获取已经存在的窗口
            //var window = 
            //    Resources.FindObjectsOfTypeAll<BuffConfigsWindow>().FirstOrDefault();
            //// 如果存在窗口则清空窗口后根据asset加载数据并聚焦到这个窗口上 这里清空的是外层窗口内部的GraphView不受影响
            //if (window != null)
            //{
            //    window.Clear();
            //    window.LoadAsset(asset);
            //    window.Focus();
            //    return;
            //}

            var window = CreateWindow<BuffConfigsWindow>(typeof(BuffConfigsWindow));
            window.Focus();
            //window.titleContent = new GUIContent(asset.name, EditorGUIUtility.GetIconForObject(asset));
            //window.LoadAsset(asset);
        }


        private async UniTask<BuffsConfig> LoadBuffs()
        {
            // TODO: 资源热更方案待定——原通过 Addressables 加载 key="BuffConfig" 配表的逻辑已移除。
            // 新资源系统接入后，于此加载并返回 BuffsConfig。
            await UniTask.CompletedTask;
            return null;
        }


        private async void OnEnable()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Framework/Buff/Editor/BuffConfigsWindow.uxml");
            tree.CloneTree(rootVisualElement);

            var system = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Framework/Buff/Editor/Style.uss");
            rootVisualElement.styleSheets.Add(system);

            BuffConfigs = new List<BuffConfigData>();
            var buffsConfig = await LoadBuffs();

            // TODO: 资源热更方案待接入，buffsConfig 为 null 时跳过配表加载
            if (buffsConfig != null)
            {
                foreach (var buff in buffsConfig.buffs)
                {
                    BuffConfigs.Add(buff);
                }
            }



            #region ListView
            Func<VisualElement> makeItem = () =>
            {
                var buffConfigVisualElement = new BuffConfigVisualElement();
                return buffConfigVisualElement;
            };

            Action<VisualElement, int> bindItem = (e, i) => BindItem(e as BuffConfigVisualElement, i);
            
            // 获取 ListView（必须在 UXML 里存在）
            listView = rootVisualElement.Q<ListView>("BuffListScroll");

            // 设置 ListView 行为
            listView.reorderable = false;
            listView.showBorder = true;
            listView.fixedItemHeight = 55;

            // 绑定创建 & 绑定逻辑
            listView.makeItem = () => new BuffConfigVisualElement();
            listView.bindItem = (e, i) =>
            {
                var elem = e as BuffConfigVisualElement;
                var label = elem.Q<Label>("nameLabel");
                label.text = $"{BuffConfigs[i].m_Buff_Id}  {BuffConfigs[i].m_Buff_Name}";
            };

            // 设置数据源
            listView.itemsSource = BuffConfigs;

            // ⭐ 关键：强制刷新
            listView.RefreshItems();

            // 选择事件
            listView.selectionChanged += OnSelectItem;
            #endregion

            #region Button
            button = this.rootVisualElement.Q<Button>("CreateButton");
            button.clicked += OnCreateButtonClick;
            #endregion

            #region BuffInfoElement
            buffInfo = this.rootVisualElement.Q<VisualElement>("BuffInfo");
            #endregion
        }

        private void OnDisable()
        {
            BuffConfigs.Clear();
            listView.Clear();
        }

        private void OnCreateButtonClick()
        {
            Debug.Log("CreateButtonClick");
        }

        private VisualElement MakeListItem()
        {
            var label = new BuffConfigVisualElement();
            return label;
        }

        // Bind the data (characterInfo) to the display (elem).
        private void BindItem(BuffConfigVisualElement elem, int i)
        {
            var label = elem.Q<Label>(name: "nameLabel");
            label.text = $"{BuffConfigs[i].m_Buff_Id}  {BuffConfigs[i].m_Buff_Name}";
        }

        private void OnSelectItem(IEnumerable<object> selectedItems)
        {
            if (buffDataView == null)
            {
                buffDataView = new BuffDataView();
                buffInfo.Add(buffDataView);
            }

            var enumerator = selectedItems.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var data = (BuffConfigData)enumerator.Current;
                buffDataView.SetBuffData(data);
            }
        }
    }

    public class BuffConfigVisualElement : VisualElement
    {
        // Use Constructor when the ListView uses makeItem and returns a VisualElement to be 
        // bound to a CharacterInfo data class.
        public BuffConfigVisualElement()
        {
            var root = new VisualElement();

            // The code below to style the ListView is for demo purpose. It's better to use a USS file
            // to style a visual element. 
            root.style.paddingTop = 3f;
            root.style.paddingRight = 0f;
            root.style.paddingBottom = 15f;
            root.style.paddingLeft = 3f;
            root.style.borderBottomColor = Color.gray;
            root.style.borderBottomWidth = 1f;
            var nameLabel = new Label() { name = "nameLabel" };
            nameLabel.style.fontSize = 14f;
            
            root.Add(nameLabel);
            Add(root);
        }
    }
}
