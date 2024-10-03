using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
        public static IAsyncHandlerDownload InitAsync_Release(string url)
        {
            // 先下载目录文件
            Dictionary<string, List<AssetAlias>> assetsAlias = null;
            MutiDownloadHandler mutiDownloadHandler = new MutiDownloadHandler();
            LoadAliasAsync(url + "/alias.json").Then((handler) =>
            {
                string json = handler.ContentText;
                assetsAlias = JsonConvert.DeserializeObject<Dictionary<string, List<AssetAlias>>>(json);
                foreach (var packageInfo in assetsAlias)
                {
                    string packageName = packageInfo.Key;
                    var groupNames = packageInfo.Value
                        .GroupBy(alia => alia.BundleName)
                        .Select(group => group.Key);
                    foreach (var groupName in groupNames)
                    {
                        string webPath = Path.Combine(url, packageName, groupName);
                        string filename = Path.Combine("BundleCache", groupName);
                        DiskFileDownloadHandler downloadHandler = new(webPath, filename);
                        mutiDownloadHandler.AddDependency(downloadHandler);
                    }
                }
                mutiDownloadHandler.Start();
            });
            mutiDownloadHandler.Then(() =>
            {
                // 所有AB包下载完成后，创建AssetPackage
                // AB包的加载要在实际加载资源时延迟加载
                _defaultAssetPackage = null;
                _assetPackages.Clear();
                foreach (var packageInfo in assetsAlias)
                {
                    string packageName = packageInfo.Key;
                    _assetPackages[packageName] = new AssetPackage_Release(packageName, assetsAlias[packageName]);
                }
            });
            return mutiDownloadHandler;
        }

#if UNITY_EDITOR
        public static IAsyncHandlerDownload InitAsync_Editor()
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
            _defaultAssetPackage = null;
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
            if (_defaultAssetPackage != null)
                Debug.Log("默认资源包：" + defaultPackageName);
            else
                Debug.LogError("资源包不存在：" + defaultPackageName);
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

        public static void LoadAssetAsync<T>(string id, UnityAction<T> callback) where T : Object
        {
            CheckDefaultPackage();
            _defaultAssetPackage.LoadAssetAsync(id, callback);
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
