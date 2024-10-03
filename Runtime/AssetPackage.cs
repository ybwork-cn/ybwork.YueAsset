using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
        private readonly Dictionary<string, Object> _assetCache = new();
        private readonly List<AssetAlias> _alias;

        internal AssetPackage_Release(string packageName, List<AssetAlias> assets)
            : base(packageName)
        {
            _alias = assets;
        }

        public override T LoadAssetSync<T>(string id)
        {
            // 查找到对象缓存，直接从缓存返回
            if (_assetCache.TryGetValue(id, out Object obj))
                return (T)obj;

            // 未加载AB包，先加载AB包
            AssetAlias assetAlias = _alias.FirstOrDefault(alia => id == alia.Name);
            if (assetAlias == null)
            {
                throw new KeyNotFoundException("资源不存在：" + id);
            }

            string bundleName = assetAlias.BundleName;
            if (!_bundles.TryGetValue(bundleName, out AssetBundle assetBundle))
            {
                string filename = Path.Combine("BundleCache", bundleName);
                assetBundle = AssetBundle.LoadFromFile(filename);
                _bundles[bundleName] = assetBundle;
            }

            return LoadAssetFromAssetBundle<T>(assetBundle, assetAlias);
        }

        // 从AB包中加载资源并缓存
        private T LoadAssetFromAssetBundle<T>(AssetBundle assetBundle, AssetAlias alias) where T : Object
        {
            T result = assetBundle.LoadAsset<T>(alias.AssetPath);
            _assetCache[alias.Name] = result;
            return result;
        }
    }

#if UNITY_EDITOR
    internal class AssetPackage_Editor : AssetPackage
    {
        private readonly Dictionary<string, string> _assetPaths = new();

        internal AssetPackage_Editor(string packageName, List<AssetAlias> assets) : base(packageName)
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
            return result;
        }
    }
#endif
}
