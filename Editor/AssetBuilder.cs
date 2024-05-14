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
        internal static string OutputPath => Application.streamingAssetsPath + "/Bundles";

        internal static void BuildAssetBundle(AssetCollectorData collectorData, string jsonFileName)
        {
            Debug.Log("开始打Windows资源包");
            Directory.CreateDirectory(OutputPath);

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
                string path = $"{OutputPath}/{packageData.PackageName}/";
                Directory.CreateDirectory(path);
                BuildPipeline.BuildAssetBundles(path, bundles.ToArray(), BuildAssetBundleOptions.None, collectorData.BuildTarget);
                Debug.Log($"{path}打包完成");
            }

            AssetBundleBuild[] dictBuild = new AssetBundleBuild[]
            {
                new() {
                    assetBundleName = "dict.ab",
                    assetNames = new string[]{ jsonFileName },
                }
            };
            Directory.CreateDirectory(OutputPath + "/__dict__/");
            BuildPipeline.BuildAssetBundles(OutputPath + "/__dict__/", dictBuild, BuildAssetBundleOptions.None, collectorData.BuildTarget);
            Debug.Log("资源包 打包完成");
        }
    }
}
