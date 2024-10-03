using Newtonsoft.Json;
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
            string outputPath = collectorData.TargetPath;
            if (!Directory.Exists(outputPath))
            {
                Debug.LogError($"路径不存在 at \"{outputPath}\"");
                return;
            }

            Debug.Log("开始打Windows资源包");
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);

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
                BuildPipeline.BuildAssetBundles(path, bundles.ToArray(), BuildAssetBundleOptions.None, collectorData.BuildTarget);
                Debug.Log($"{path}打包完成");
            }

            string aliasMap = JsonConvert.SerializeObject(collectorData.GetAssets(), Formatting.Indented);
            File.WriteAllText(Path.Combine(outputPath, "alias.json"), aliasMap);

            Debug.Log("资源包 打包完成");
        }
    }
}
