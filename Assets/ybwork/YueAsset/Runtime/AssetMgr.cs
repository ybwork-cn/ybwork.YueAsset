using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ybwork.Assets
{
    public static class AssetMgr
    {
        public static readonly string AssetBundlePath = Application.streamingAssetsPath + "/Bundles/";
        public const string AliasPath = "Assets/Settings/AssetCollectorData.alias.json";
        private static AssetGroup _defaultAssetGroup = null;
        private static readonly Dictionary<string, AssetGroup> _assetGroups = new();

        public static void InitSync()
        {
            LoadGroups();
            foreach (AssetGroup group in _assetGroups.Values)
                group.InitSync();
        }

        public static async Task InitAsync()
        {
            LoadGroups();
            foreach (AssetGroup group in _assetGroups.Values)
                await group.InitAsync();
        }

        public static void SetDefaultGroup(string defaultGroupName)
        {
            _defaultAssetGroup = GetGroup(defaultGroupName);
        }

        public static AssetGroup GetGroup(string groupName)
        {
            _assetGroups.TryGetValue(groupName, out AssetGroup group);
            return group;
        }

        public static T LoadAssetSync<T>(string id) where T : Object
        {
            CheckDefaultGroup();
            return _defaultAssetGroup.LoadAssetSync<T>(id);
        }

        private static Dictionary<string, List<AssetAlias>> LoadAlias()
        {
#if UNITY_EDITOR
            string json = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(AliasPath).text;
#else
            AssetBundle bundle = AssetBundle.LoadFromFile(AssetBundlePath + "__dict__/dict.ab");
            string json = bundle.LoadAllAssets<TextAsset>()[0].text;
#endif
            Dictionary<string, List<AssetAlias>> assetsAlias = JsonConvert.DeserializeObject<Dictionary<string, List<AssetAlias>>>(json);
            return assetsAlias;
        }

        private static void LoadGroups()
        {
            Dictionary<string, List<AssetAlias>> assetsAlias = LoadAlias();
            _assetGroups.Clear();
            foreach (var groupName in assetsAlias.Keys)
            {
#if UNITY_EDITOR
                _assetGroups.Add(groupName, new AssetGroup_Editor(groupName, assetsAlias[groupName]));
#else
                _assetGroups.Add(groupName, new AssetGroup_Release(groupName, assetsAlias[groupName]));
#endif
            }
        }

        private static void CheckDefaultGroup()
        {
            if (_defaultAssetGroup == null)
                throw new MissingReferenceException("Please Call AssetMgr.SetDefaultGroup");
        }
    }
}
