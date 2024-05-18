using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ybwork.Assets.Editor
{
    internal static class AssetBuilder
    {
        internal static void BuildAssetBundle(AssetCollectorData collectorData)
        {
            string outputPath = Path.Combine(Environment.CurrentDirectory, "YueAssets");

            Debug.Log("开始打Windows资源包");
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);

            Dictionary<string, Dictionary<string, BundleGroupInfo>> packgeInfos = new();
            foreach (AssetCollectorPackageData packageData in collectorData.Packages)
            {
                List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();
                foreach (AssetCollectorGroupData groupData in packageData.Groups)
                {
                    AssetBundleBuild build = new()
                    {
                        assetBundleName = groupData.GroupName + ".ab",
                        assetNames = groupData.GetAssetPaths().ToArray()
                    };
                    bundles.Add(build);
                }
                string path = Path.Combine(outputPath, packageData.PackageName);
                Directory.CreateDirectory(path);
                AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(path, bundles.ToArray(), BuildAssetBundleOptions.None, collectorData.BuildTarget);
                Debug.Log($"{path}打包完成");


                Dictionary<string, BundleGroupInfo> groupInfos = new();
                foreach (string group in assetBundleManifest.GetAllAssetBundles())
                {
                    groupInfos.Add(group, new BundleGroupInfo
                    {
                        Hash = assetBundleManifest.GetAssetBundleHash(group).ToString(),
                        Size = (ulong)new FileInfo(Path.Combine(outputPath, packageData.PackageName, group)).Length,
                    });
                }
                packgeInfos.Add(packageData.PackageName, groupInfos);
            }

            string aliasMap = JsonConvert.SerializeObject(collectorData.GetAssets(), Formatting.Indented);
            File.WriteAllText(Path.Combine(outputPath, "alias.json"), aliasMap);

            string catalog = JsonConvert.SerializeObject(packgeInfos, Formatting.Indented);
            File.WriteAllText(Path.Combine(outputPath, "catalog.json"), catalog);

            Debug.Log("资源包 打包完成");
        }
    }
}
