using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ybwork.Assets
{
    public abstract class AssetGroup
    {
        public readonly string GroupName;

        internal AssetGroup(string groupName)
        {
            GroupName = groupName;
        }

        public abstract void InitSync();
        public abstract Task InitAsync();
        public abstract T LoadAssetSync<T>(string id) where T : Object;
    }

    internal class AssetGroup_Release : AssetGroup
    {
        private readonly Dictionary<string, AssetBundle> _assetBundles = new();
        private readonly Dictionary<string, AssetBundle> _assetPaths = new();
        private readonly List<AssetAlias> _assets;
        private bool _inited = false;

        internal AssetGroup_Release(string groupName, List<AssetAlias> assets) : base(groupName)
        {
            _assets = assets;
        }

        public override void InitSync()
        {
            if (_inited)
                return;

            var bundleNames = _assets.GroupBy(asset => asset.BundleName).Select(group => group.Key);
            foreach (var bundleName in bundleNames)
            {
                string path = AssetMgr.AssetBundlePath + GroupName + "/" + bundleName;
                _assetBundles[bundleName] = AssetBundle.LoadFromFile(path + ".ab");
            }

            foreach (var asset in _assets)
            {
                _assetPaths[asset.Name] = _assetBundles[asset.BundleName];
            }

            _inited = true;
        }

        public override async Task InitAsync()
        {
            if (_inited)
                return;

            var bundleNames = _assets.GroupBy(asset => asset.BundleName).Select(group => group.Key);
            foreach (var bundleName in bundleNames)
            {
                string path = AssetMgr.AssetBundlePath + GroupName + "/" + bundleName;
                var assetBundleCreateRequest = await AssetBundle.LoadFromFileAsync(path + ".ab");
                _assetBundles[bundleName] = assetBundleCreateRequest.assetBundle;
            }

            foreach (var asset in _assets)
            {
                _assetPaths[asset.Name] = _assetBundles[asset.BundleName];
            }

            _inited = true;
        }

        public override T LoadAssetSync<T>(string id)
        {
            if (!_inited)
                throw new System.Exception("Not Inited");
            return _assetPaths[id].LoadAsset<T>(id);
        }
    }

    internal class AssetGroup_Editor : AssetGroup
    {
        private readonly Dictionary<string, string> _assetPaths = new();

        internal AssetGroup_Editor(string groupName, List<AssetAlias> assets) : base(groupName)
        {
            foreach (AssetAlias asset in assets)
            {
                _assetPaths.Add(asset.Name, asset.AssetPath);
            }
        }

        public override void InitSync()
        {
        }

        public override Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public override T LoadAssetSync<T>(string id)
        {
            if (!_assetPaths.TryGetValue(id, out string path))
            {
                throw new System.IndexOutOfRangeException("不存在的资源名称:" + id);
            }
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return null;
#endif
        }
    }
}
