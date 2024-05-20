using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ybwork.Assets
{
    public abstract class AssetPackage
    {
        public readonly string PackageName;

        internal AssetPackage(string packageName)
        {
            PackageName = packageName;
        }

        public abstract T LoadAssetSync<T>(string id) where T : Object;
    }

    internal class AssetPackage_Release : AssetPackage
    {
        private readonly Dictionary<string, AssetBundle> _bundles = new();
        private readonly Dictionary<string, AssetBundle> _assetInBundles = new();
        private readonly Dictionary<string, Object> _assets = new();
        private readonly List<AssetAlias> _alias;
        private readonly Dictionary<string, BundleGroupInfo> _groupInfos;
        private readonly MutiDownloadHandler _downloadHandler = new MutiDownloadHandler();

        internal AssetPackage_Release(string packageName, List<AssetAlias> assets, Dictionary<string, BundleGroupInfo> groupInfos)
            : base(packageName)
        {
            _alias = assets;
            _groupInfos = groupInfos;
        }

        public MutiDownloadHandler InitAsync(string url)
        {
            var bundleNames = _alias.GroupBy(asset => asset.BundleName).Select(group => group.Key);
            foreach (var bundleName in bundleNames)
            {
                BundleGroupInfo bundleGroupInfo = _groupInfos[bundleName];
                AssetBundleDownloadHandler downloadHandler = new(url, PackageName, bundleName, bundleGroupInfo);
                downloadHandler.Then(() =>
                {
                    _bundles[bundleName] = downloadHandler.AssetBundle;
                });
                _downloadHandler.AddDependency(downloadHandler);
            }
            _downloadHandler.Start();

            _downloadHandler.Then(() =>
            {
                foreach (var asset in _alias)
                {
                    _assetInBundles[asset.Name] = _bundles[asset.BundleName];
                }
            });

            return _downloadHandler;
        }

        public override T LoadAssetSync<T>(string id)
        {
            if (_downloadHandler == null)
                throw new System.Exception("Not Inited");

            if (_assets.TryGetValue(id, out Object obj))
                return (T)obj;

            string path = _alias.First(alia => id == alia.Name).AssetPath;
            T result = _assetInBundles[id].LoadAsset<T>(path);
            result.name = id;

            _assets[id] = result;
            return result;
        }
    }

#if UNITY_EDITOR
    internal class AssetPackage_Editor : AssetPackage
    {
        private readonly Dictionary<string, string> _assetPaths = new();

        internal AssetPackage_Editor(string groupName, List<AssetAlias> assets) : base(groupName)
        {
            foreach (AssetAlias asset in assets)
            {
                _assetPaths.Add(asset.Name, asset.AssetPath);
            }
        }

        public override T LoadAssetSync<T>(string id)
        {
            if (!_assetPaths.TryGetValue(id, out string path))
            {
                throw new System.IndexOutOfRangeException("不存在的资源名称:" + id);
            }
            T result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            result.name = id;
            return result;
        }
    }
#endif
}
