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
        private static AssetPackage _defaultAssetPackage = null;
        private static readonly Dictionary<string, AssetPackage> _assetPackages = new();

        public static void InitSync()
        {
            LoadPackages();
            foreach (AssetPackage package in _assetPackages.Values)
                package.InitSync();
        }

        public static async Task InitAsync()
        {
            LoadPackages();
            foreach (AssetPackage package in _assetPackages.Values)
                await package.InitAsync();
        }

        public static void SetDefaultPackage(string defaultPackageName)
        {
            _defaultAssetPackage = GetPackage(defaultPackageName);
        }

        public static AssetPackage GetPackage(string packageName)
        {
            _assetPackages.TryGetValue(packageName, out AssetPackage package);
            return package;
        }

        public static T LoadAssetSync<T>(string id) where T : Object
        {
            CheckDefaultPackage();
            return _defaultAssetPackage.LoadAssetSync<T>(id);
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

        private static void LoadPackages()
        {
            Dictionary<string, List<AssetAlias>> assetsAlias = LoadAlias();
            _assetPackages.Clear();
            foreach (var packageName in assetsAlias.Keys)
            {
#if UNITY_EDITOR
                _assetPackages.Add(packageName, new AssetPackage_Editor(packageName, assetsAlias[packageName]));
#else
                _assetPackages.Add(packageName, new AssetPackage_Release(packageName, assetsAlias[packageName]));
#endif
            }
        }

        private static void CheckDefaultPackage()
        {
            if (_defaultAssetPackage == null)
                throw new MissingReferenceException("Please Call AssetMgr.SetDefaultPackage");
        }
    }
}
