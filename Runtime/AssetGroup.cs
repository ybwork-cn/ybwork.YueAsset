using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ybwork.Assets
{
    public abstract class AssetPackage
    {
        public readonly string PackageName;

        internal AssetPackage(string packageName)
        {
            PackageName = packageName;
        }

        public abstract Task InitAsync(string url);
        public abstract T LoadAssetSync<T>(string id) where T : Object;
    }

    internal class AssetPackage_Release : AssetPackage
    {
        private readonly Dictionary<string, AssetBundle> _bundles = new();
        private readonly Dictionary<string, AssetBundle> _assetInBundles = new();
        private readonly Dictionary<string, Object> _assets = new();
        private readonly List<AssetAlias> _alias;
        private bool _inited = false;

        internal AssetPackage_Release(string packageName, List<AssetAlias> assets) : base(packageName)
        {
            _alias = assets;
        }

        public override async Task InitAsync(string url)
        {
            if (_inited)
                return;

            var bundleNames = _alias.GroupBy(asset => asset.BundleName).Select(group => group.Key);
            foreach (var bundleName in bundleNames)
            {
                string path = Path.Combine(url, PackageName, bundleName.ToLower() + ".ab");
                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path);
                await request.SendWebRequest();
                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);
                _bundles[bundleName] = assetBundle;
            }

            foreach (var asset in _alias)
            {
                _assetInBundles[asset.Name] = _bundles[asset.BundleName];
            }

            _inited = true;
        }

        public override T LoadAssetSync<T>(string id)
        {
            if (!_inited)
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

        public override Task InitAsync(string url)
        {
            return Task.CompletedTask;
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
