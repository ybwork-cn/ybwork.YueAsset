using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace ybwork.Assets
{
    public static class AssetMgr
    {
        internal static readonly string PersistentDataPath = Application.persistentDataPath + "/YueAsset/Bundles";
        private static AssetPackage _defaultAssetPackage = null;
        private static readonly Dictionary<string, AssetPackage> _assetPackages = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">资源包文件夹URL</param>
        /// <returns></returns>
        public static IAsyncHandler InitAsync_Release(string url)
        {
            DownloadHandler aliasDownloadHandler = LoadAliasAsync(url + "/alias.json");

            MutiDownloadHandler mutiAsyncHandler = new MutiDownloadHandler(aliasDownloadHandler);

            aliasDownloadHandler.Then((handler) =>
            {
                Dictionary<string, List<AssetAlias>> assetsAlias =
                    JsonConvert.DeserializeObject<Dictionary<string, List<AssetAlias>>>(handler.ContentText);
                _assetPackages.Clear();
                foreach (var packageName in assetsAlias.Keys)
                {
                    AssetPackage_Release package = new AssetPackage_Release(packageName, assetsAlias[packageName]);
                    _assetPackages.Add(packageName, package);

                    var packageDownloadHandler = package.InitAsync(url);
                    mutiAsyncHandler.AddDependency(packageDownloadHandler);
                }
            });

            return mutiAsyncHandler;
        }

#if UNITY_EDITOR
        public static IAsyncHandler InitAsync_Editor()
        {
            return InitAsync_Editor(Application.dataPath + "/YueAssetAlias.json");
        }

        public static IAsyncHandler InitAsync_Editor(string alias_path)
        {
            DownloadHandler aliasDownloadHandler = LoadAliasAsync(alias_path);
            aliasDownloadHandler.Then((handler) =>
            {
                Dictionary<string, List<AssetAlias>> assetsAlias =
                    JsonConvert.DeserializeObject<Dictionary<string, List<AssetAlias>>>(handler.ContentText);
                _assetPackages.Clear();
                foreach (var packageName in assetsAlias.Keys)
                {
                    AssetPackage_Editor package = new AssetPackage_Editor(packageName, assetsAlias[packageName]);
                    _assetPackages.Add(packageName, package);
                }
            });

            return aliasDownloadHandler;
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

        private static DownloadHandler LoadAliasAsync(string alias_url)
        {
            return new DownloadHandler(alias_url);
        }

        private static void CheckDefaultPackage()
        {
            if (_defaultAssetPackage == null)
                throw new MissingReferenceException("Please Call AssetMgr.SetDefaultPackage");
        }
    }
}
