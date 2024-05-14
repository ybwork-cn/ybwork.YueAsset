using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ybwork.Assets
{
    public static class AssetMgr
    {
        private static AssetPackage _defaultAssetPackage = null;
        private static readonly Dictionary<string, AssetPackage> _assetPackages = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">资源包文件夹URL</param>
        /// <returns></returns>
        public static async Task InitAsync_Release(string url)
        {
            await LoadReleasePackages(url + "/alias.json");
            foreach (AssetPackage package in _assetPackages.Values)
                await package.InitAsync(url);
        }

        private static async Task LoadReleasePackages(string alias_url)
        {
            Dictionary<string, List<AssetAlias>> assetsAlias = await LoadAlias(alias_url);
            _assetPackages.Clear();
            foreach (var packageName in assetsAlias.Keys)
            {
                _assetPackages.Add(packageName, new AssetPackage_Release(packageName, assetsAlias[packageName]));
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias_url">alias目录url</param>
        /// <returns></returns>
        public static async Task InitAsync_Editor(string alias_url)
        {
            await LoadEditorPackages(alias_url);
            foreach (AssetPackage package in _assetPackages.Values)
                await package.InitAsync(null);
        }

        private static async Task LoadEditorPackages(string alias_url)
        {
            Dictionary<string, List<AssetAlias>> assetsAlias = await LoadAlias(alias_url);
            _assetPackages.Clear();
            foreach (var packageName in assetsAlias.Keys)
            {
                _assetPackages.Add(packageName, new AssetPackage_Editor(packageName, assetsAlias[packageName]));
            }
        }
#endif

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

        private static async Task<Dictionary<string, List<AssetAlias>>> LoadAlias(string alias_url)
        {
            UnityWebRequest requset = UnityWebRequest.Get(alias_url);
            await requset.SendWebRequest();
            string text = requset.downloadHandler.text;
            return JsonConvert.DeserializeObject<Dictionary<string, List<AssetAlias>>>(text);
        }

        private static void CheckDefaultPackage()
        {
            if (_defaultAssetPackage == null)
                throw new MissingReferenceException("Please Call AssetMgr.SetDefaultPackage");
        }
    }
}
