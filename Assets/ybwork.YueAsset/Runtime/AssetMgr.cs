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
        public static IAsyncDownloadHandler InitAsync_Release(string url)
        {
            DownloadHandler aliasDownloadHandler = LoadAliasAsync(url + "/alias.json");
            DownloadHandler catalogDownloadHandler = LoadAliasAsync(url + "/catalog.json");
            MutiDownloadHandler mutiAsyncHandler = new MutiDownloadHandler();
            mutiAsyncHandler.AddDependency(aliasDownloadHandler);
            mutiAsyncHandler.AddDependency(catalogDownloadHandler);
            mutiAsyncHandler.Start();

            MutiDownloadHandler initAsyncHandler = new MutiDownloadHandler(mutiAsyncHandler);

            mutiAsyncHandler.Then(() =>
            {
                Dictionary<string, Dictionary<string, BundleGroupInfo>> packgeInfos =
                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, BundleGroupInfo>>>(catalogDownloadHandler.ContentText);

                Dictionary<string, List<AssetAlias>> assetsAlias =
                    JsonConvert.DeserializeObject<Dictionary<string, List<AssetAlias>>>(aliasDownloadHandler.ContentText);
                _assetPackages.Clear();
                foreach (var packageAlias in assetsAlias)
                {
                    string packageName = packageAlias.Key;
                    AssetPackage_Release package = new AssetPackage_Release(packageName, assetsAlias[packageName], packgeInfos[packageName]);
                    _assetPackages.Add(packageName, package);

                    var packageDownloadHandler = package.InitAsync(url);
                    initAsyncHandler.AddDependency(packageDownloadHandler);
                }
                initAsyncHandler.Start();
            });

            return initAsyncHandler;
        }

#if UNITY_EDITOR
        public static IAsyncHandler InitAsync_Editor()
        {
            AssetCollectorData collectorData = AssetCollectorData.GetData();
            string aliasMap = JsonConvert.SerializeObject(collectorData.GetAssets(), Formatting.Indented);
            InitEditor(aliasMap);
            return new CompoletedAsyncHandler();
        }

        private static void InitEditor(string aliasMap)
        {
            Dictionary<string, List<AssetAlias>> assetsAlias =
                                JsonConvert.DeserializeObject<Dictionary<string, List<AssetAlias>>>(aliasMap);
            _assetPackages.Clear();
            foreach (var packageName in assetsAlias.Keys)
            {
                AssetPackage_Editor package = new AssetPackage_Editor(packageName, assetsAlias[packageName]);
                _assetPackages.Add(packageName, package);
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
