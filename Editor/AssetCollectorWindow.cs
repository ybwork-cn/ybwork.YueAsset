using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using ObjectField = UnityEditor.UIElements.ObjectField;

namespace ybwork.Assets.Editor
{
    internal class AssetCollectorWindow : EditorWindow
    {
        private const string _jsonFilename = "Assets/Settings/AssetCollectorData.alias.json";
        private static AssetCollectorWindow _window;
        private AssetCollectorData _data;

        [MenuItem("ybwork/Assets/AssetCollectorWindow")]
        public static void OpenWindow()
        {
            if (_window == null)
            {
                _window = GetWindow<AssetCollectorWindow>();
                if (_window == null)
                    _window = CreateInstance<AssetCollectorWindow>();
            }
            _window.Show();
        }

        private void Awake()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(AssetCollectorData).FullName}");
            if (guids.Length == 0)
            {
                _data = CreateInstance<AssetCollectorData>();
                Directory.CreateDirectory("Assets/Settings/");
                AssetDatabase.CreateAsset(_data, "Assets/Settings/" + nameof(AssetCollectorData) + ".asset");
            }
            else if (guids.Length == 1)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                _data = AssetDatabase.LoadAssetAtPath<AssetCollectorData>(assetPath);
            }
            else
            {
                string message = "存在多个" + nameof(AssetCollectorData) + "at";
                foreach (var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    message += "\r\n\t" + assetPath;
                }
                Debug.LogWarning(message);
            }
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // 全局搜索并加载
            string[] guids = AssetDatabase.FindAssets(nameof(AssetCollectorWindow));
            VisualTreeAsset treeAsset = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(assetPath => AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(VisualTreeAsset))
                .Select(assetPath => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath))
                .First();
            treeAsset.CloneTree(root);

            ListView packageListView = root.Q<ListView>("PackageListView");
            ListView groupListView = root.Q<ListView>("GroupListView");
            ScrollView collectorScrollView = root.Q<ScrollView>("CollectorScrollView");

            TextField packageName = root.Q<TextField>("PackageName");
            packageName.SetEnabled(false);
            packageName.RegisterValueChangedCallback(evt =>
            {
                AssetCollectorPackageData package = _data.Packages[packageListView.selectedIndex];
                package.PackageName = evt.newValue;
                packageListView.RefreshItem(packageListView.selectedIndex);
            });

            TextField groupName = root.Q<TextField>("GroupName");
            groupName.SetEnabled(false);
            groupName.RegisterValueChangedCallback(evt =>
            {
                AssetCollectorPackageData package = _data.Packages[packageListView.selectedIndex];
                AssetCollectorGroupData group = package.Groups[groupListView.selectedIndex];
                group.GroupName = evt.newValue;

                RefreshCollectorList(collectorScrollView, group);
                groupListView.RefreshItem(groupListView.selectedIndex);
            });

            packageListView.makeItem = MakeListViewItem;
            packageListView.bindItem = (element, index) =>
            {
                Label label = element.Q<Label>("Label1");
                label.text = _data.Packages[index].PackageName;
            };
            packageListView.itemsAdded += (itemIndices) =>
            {
                foreach (int index in itemIndices)
                    _data.Packages[index] = new AssetCollectorPackageData { PackageName = "DefaultPackage" };
                packageListView.SetSelection(_data.Packages.Count - 1);
            };
            packageListView.itemsRemoved += (itemIndices) =>
            {
                packageName.SetEnabled(false);
                packageName.SetValueWithoutNotify("");
                packageListView.SetSelection(-1);
            };
            packageListView.selectedIndicesChanged += (IEnumerable<int> indices) =>
            {
                packageName.SetEnabled(false);
                packageName.SetValueWithoutNotify("");
                groupListView.ClearSelection();

                if (packageListView.selectedIndex < 0)
                    return;

                packageName.SetEnabled(true);
                AssetCollectorPackageData package = _data.Packages[packageListView.selectedIndex];
                packageName.SetValueWithoutNotify(package.PackageName);

                ResetListViewDataSource(groupListView, package.Groups);
            };

            groupListView.makeItem = MakeListViewItem;
            groupListView.bindItem = (element, index) =>
            {
                Label label = element.Q<Label>("Label1");
                label.text = _data.Packages[packageListView.selectedIndex].Groups[index].GroupName;
            };
            groupListView.itemsAdded += (itemIndices) =>
            {
                AssetCollectorPackageData package = _data.Packages[packageListView.selectedIndex];
                foreach (int index in itemIndices)
                    package.Groups[index] = new AssetCollectorGroupData { GroupName = "DefaultGroup" };
                groupListView.SetSelection(package.Groups.Count - 1);
            };
            groupListView.itemsRemoved += (itemIndices) =>
            {
                groupName.SetEnabled(false);
                groupName.SetValueWithoutNotify("");
                groupListView.SetSelection(-1);
            };
            groupListView.selectedIndicesChanged += (IEnumerable<int> indices) =>
            {
                groupName.SetEnabled(false);
                groupName.SetValueWithoutNotify("");
                collectorScrollView.Clear();

                AssetCollectorPackageData package = _data.Packages[packageListView.selectedIndex];
                AssetCollectorGroupData group = null;
                if (package != null && groupListView.selectedIndex >= 0 && groupListView.selectedIndex < package.Groups.Count)
                    group = package.Groups[groupListView.selectedIndex];
                if (group == null)
                    return;

                groupName.SetEnabled(true);
                groupName.SetValueWithoutNotify(group.GroupName);
                RefreshCollectorList(collectorScrollView, group);
            };

            VisualElement draggableArea = root.Q("DraggableArea");

            // 注册拖拽进入、拖拽离开、拖拽过程中和拖拽释放的回调
            //draggableArea.RegisterCallback<DragEnterEvent>(evt => DragAndDrop.visualMode = DragAndDropVisualMode.Generic);
            draggableArea.RegisterCallback<DragLeaveEvent>(evt => DragAndDrop.visualMode = DragAndDropVisualMode.None);
            draggableArea.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.None;

                int selectedPackageIndex = packageListView.selectedIndex;
                int selectedGroupIndex = groupListView.selectedIndex;
                if (selectedPackageIndex < 0 || selectedPackageIndex >= _data.Packages.Count)
                    return;
                AssetCollectorPackageData package = _data.Packages[packageListView.selectedIndex];
                if (selectedGroupIndex < 0 || selectedGroupIndex >= package.Groups.Count)
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            });
            draggableArea.RegisterCallback<DragPerformEvent>(evt =>
            {
                int selectedPackageIndex = packageListView.selectedIndex;
                int selectedGroupIndex = groupListView.selectedIndex;

                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    AssetCollectorItemData collectorItemData = CreateCollectorItemData(obj);
                    _data.Packages[selectedPackageIndex].Groups[selectedGroupIndex].Items.Add(collectorItemData);
                }

                RefreshCollectorList(collectorScrollView, _data.Packages[selectedPackageIndex].Groups[selectedGroupIndex]);
            });

            Button refreshButton = root.Query<Button>("RefreshButton");
            refreshButton.clicked += () =>
            {
                int selectedPackageIndex = packageListView.selectedIndex;
                int selectedGroupIndex = groupListView.selectedIndex;

                RebuildListView(packageListView);

                AssetCollectorPackageData package = null;
                if (selectedPackageIndex >= 0 && selectedPackageIndex < _data.Packages.Count)
                    package = _data.Packages[selectedPackageIndex];
                ResetListViewDataSource(groupListView, package?.Groups);

                AssetCollectorGroupData group = null;
                if (package != null && selectedGroupIndex >= 0 && selectedGroupIndex < package.Groups.Count)
                    group = package.Groups[selectedGroupIndex];
                if (group != null)
                    RefreshCollectorList(collectorScrollView, group);
            };

            EnumField enumBuildTarget = root.Query<EnumField>("BuildTarget");
            enumBuildTarget.value = _data.BuildTarget;
            enumBuildTarget.RegisterValueChangedCallback(e =>
            {
                _data.BuildTarget = (BuildTarget)e.newValue;
            });

            // 保存按钮
            Button saveButton = root.Query<Button>("SaveButton");
            saveButton.clicked += SaveData;

            // 保存按钮
            Button buildButton = root.Query<Button>("BuildButton");
            buildButton.clicked += Build;

            ResetListViewDataSource(packageListView, _data.Packages);
        }

        private void OnGUI()
        {
            // 检测按键事件
            if (Event.current.type == EventType.KeyDown)
            {
                // 如果按下的是Ctrl+S（在Mac上是Cmd+S）
                if ((Event.current.control || Event.current.command) && Event.current.keyCode == KeyCode.S)
                {
                    SaveData();
                    Event.current.Use();
                }
            }
        }

        private void SaveData()
        {
            EditorUtility.SetDirty(_data);
            AssetDatabase.SaveAssets();

            string contents = JsonConvert.SerializeObject(_data.GetAssets(), Formatting.Indented);
            File.WriteAllText(_jsonFilename, contents + "\r\n");
            Debug.Log("资源收集器保存成功");
        }

        private void Build()
        {
            SaveData();
            AssetBuilder.BuildAssetBundle(_data, _jsonFilename);
        }

        private static void RebuildListView(ListView listView)
        {
            listView.Rebuild();
            if (listView.selectedIndex < 0 && listView.itemsSource != null && listView.itemsSource.Count > 0)
                listView.SetSelection(0);
        }

        private static void ResetListViewDataSource(ListView listView, IList source)
        {
            listView.itemsSource = source;
            RebuildListView(listView);
        }

        private static void RefreshCollectorList(ScrollView collectorScrollView, AssetCollectorGroupData groupData)
        {
            collectorScrollView.Clear();

            foreach (AssetCollectorItemData item in groupData.Items)
            {
                VisualElement element = MakeCollectorListViewItem(groupData, item, onDelete: () =>
                {
                    groupData.Items.Remove(item);
                    RefreshCollectorList(collectorScrollView, groupData);
                });
                collectorScrollView.Add(element);
            }
        }

        private VisualElement MakeListViewItem()
        {
            VisualElement element = new VisualElement();

            {
                var label = new Label();
                label.name = "Label1";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.flexGrow = 1f;
                label.style.height = 20f;
                label.style.marginLeft = 6f;
                element.Add(label);
            }

            return element;
        }

        private static AssetCollectorItemData CreateCollectorItemData(Object obj)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);

            AssetCollectorItemData collectorItemData = new AssetCollectorItemData();
            collectorItemData.AssetGUID = AssetDatabase.AssetPathToGUID(assetPath);
            return collectorItemData;
        }

        private static VisualElement MakeCollectorListViewItem(
            AssetCollectorGroupData groupData,
            AssetCollectorItemData collectorItemData,
            Action onDelete)
        {
            VisualElement element = new VisualElement();

            VisualElement elementTop = new VisualElement();
            elementTop.style.flexDirection = FlexDirection.Row;
            element.Add(elementTop);

            VisualElement elementSettings = new VisualElement();
            elementSettings.style.flexDirection = FlexDirection.Row;
            elementSettings.style.height = 20;
            elementSettings.style.alignItems = Align.Center;
            element.Add(elementSettings);

            VisualElement elementFoldout = new VisualElement();
            elementFoldout.style.flexDirection = FlexDirection.Row;
            element.Add(elementFoldout);

            VisualElement elementSpace = new VisualElement();
            elementSpace.style.flexDirection = FlexDirection.Column;
            element.Add(elementSpace);

            // Top VisualElement
            {
                Button button = new Button();
                button.name = "Delete";
                button.text = "-";
                button.style.width = 20;
                button.style.unityTextAlign = TextAnchor.MiddleCenter;
                button.clicked += onDelete;
                elementTop.Add(button);
            }
            {
                VisualElement objectRow = new VisualElement();
                objectRow.style.flexDirection = FlexDirection.Row;
                objectRow.style.flexGrow = 1f;

                Label label = new Label();
                label.style.width = 65;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                objectRow.Add(label);

                ObjectField objectField = new ObjectField();
                objectField.name = "ObjectField1";
                objectField.objectType = typeof(Object);
                objectField.allowSceneObjects = false;
                objectField.style.unityTextAlign = TextAnchor.MiddleLeft;
                objectField.style.flexGrow = 1f;
                objectField.value = AssetDatabase.LoadAssetAtPath<Object>(collectorItemData.AssetPath);
                RefreshAssetLabel(label, collectorItemData, objectField.value);
                objectField.RegisterValueChangedCallback(evt =>
                {
                    // register "ValuChangedEvent" to the objectField
                    // When responding, refresh the "Main Assets" Foldout
                    collectorItemData.AssetPath = AssetDatabase.GetAssetPath(evt.newValue);

                    Foldout foldout = elementFoldout.Q<Foldout>("Foldout1");
                    RefreshAssetLabel(label, collectorItemData, objectField.value);
                    RefreshAssetList(groupData, collectorItemData, foldout);
                });
                objectRow.Add(objectField);
                elementTop.Add(objectRow);
            }

            // Settings VisualElement
            {
                var label = new Label();
                label.style.width = 93;
                elementSettings.Add(label);
            }
            {
                var label = new Label("寻址方式:");
                elementSettings.Add(label);
            }
            {
                List<string> list = new List<string> { "文件名", "分组名_文件名" };
                var dropdown = new DropdownField(list, 0);
                dropdown.style.width = 100;
                dropdown.RegisterValueChangedCallback(evt =>
                {
                    collectorItemData.AssetStyle = (AssetCollectorItemStyle)list.IndexOf(evt.newValue);

                    Foldout foldout = elementFoldout.Q<Foldout>("Foldout1");
                    RefreshAssetList(groupData, collectorItemData, foldout);
                });
                elementSettings.Add(dropdown);
            }

            // Foldout VisualElement
            {
                var label = new Label();
                label.style.width = 90;
                elementFoldout.Add(label);
            }
            {
                Foldout foldout = new Foldout();
                foldout.name = "Foldout1";
                foldout.value = false;
                foldout.text = "Main Assets";
                elementFoldout.Add(foldout);

                RefreshAssetList(groupData, collectorItemData, foldout);
            }

            // Space VisualElement
            {
                var label = new Label();
                label.style.height = 10;
                elementSpace.Add(label);
            }

            return element;
        }

        private static void RefreshAssetLabel(Label label, AssetCollectorItemData collectorItemData, Object asset)
        {
            label.style.color = Color.white;
            label.text = "Collected";
            if (collectorItemData.GetRepeatedAssets().Length > 0)
            {
                label.text = "Repeat";
                label.style.color = Color.yellow;
            }
            if (asset == null)
            {
                label.text = "Missing";
                label.style.color = Color.yellow;
            }
        }

        private static void RefreshAssetList(
            AssetCollectorGroupData groupData,
            AssetCollectorItemData collectorItemData,
            Foldout foldout)
        {
            foldout.Clear();
            foreach (AssetAlias assetAlias in collectorItemData.GetAssets(groupData.GroupName))
            {
                VisualElement elementItem = new VisualElement();
                elementItem.style.flexDirection = FlexDirection.Row;

                TextField textField = new TextField();
                textField.value = assetAlias.Name;
                textField.textEdition.isReadOnly = true;
                textField.style.width = 200;
                textField.style.minWidth = 200;
                textField.style.maxWidth = 200;
                if (collectorItemData.GetRepeatedAssets().Contains(assetAlias.Name))
                {
                    textField.Children().Last().Children().First().style.color = Color.yellow;
                }
                elementItem.Add(textField);

                Label label = new Label();
                label.text = assetAlias.AssetPath;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                elementItem.Add(label);

                foldout.Add(elementItem);
            }
        }
    }
}
