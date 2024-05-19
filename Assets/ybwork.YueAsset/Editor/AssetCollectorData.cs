using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ybwork.Assets.Editor
{
    internal enum AssetCollectorItemStyle
    {
        FileName,
        GroupName_FileName,
    }

    [Serializable]
    internal class AssetCollectorItemData
    {
        public string AssetGUID;
        public AssetCollectorItemStyle AssetStyle;
        public string AssetPath
        {
            get => AssetDatabase.GUIDToAssetPath(AssetGUID);
            set
            {
                AssetGUID = AssetDatabase.AssetPathToGUID(value);
            }
        }

        public string[] GetRepeatedAssets()
        {
            return GetAssetNames()
                .GroupBy(item => item)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();
        }

        private IEnumerable<string> GetAssetNames()
        {
            return GetAssetPaths()
                .Select(path => Path.GetFileNameWithoutExtension(path));
        }

        public IEnumerable<string> GetAssetPaths()
        {
            var assetPath = AssetPath;
            if (string.IsNullOrEmpty(AssetPath))
                yield break;
            else if (AssetDatabase.IsValidFolder(assetPath))
            {
                string[] guids = AssetDatabase.FindAssets(string.Empty, new string[] { assetPath });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    // 如果路径为文件夹，则忽略此路径
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        yield return path;
                    }
                }
            }
            else
            {
                yield return assetPath;
            }
        }

        public IEnumerable<AssetAlias> GetAssets(string groupName)
        {
            return GetAssetPaths()
                .Select(path =>
                {
                    string name = Path.GetFileNameWithoutExtension(path);
                    name = AssetStyle switch
                    {
                        AssetCollectorItemStyle.FileName => name,
                        AssetCollectorItemStyle.GroupName_FileName => groupName + "_" + name,
                        _ => throw new NotImplementedException(),
                    };

                    return new AssetAlias
                    {
                        Name = name,
                        BundleName = groupName,
                        AssetPath = path,
                    };
                });
        }
    }

    [Serializable]
    internal class AssetCollectorGroupData
    {
        public string GroupName;
        public List<AssetCollectorItemData> Items = new();

        public string[] GetAssetPaths()
        {
            return Items.SelectMany(item => item.GetAssetPaths()).ToArray();
        }

        public IEnumerable<AssetAlias> GetAssets()
        {
            return Items.SelectMany(item => item.GetAssets(GroupName));
        }
    }

    [Serializable]
    internal class AssetCollectorPackageData
    {
        public string PackageName;
        public List<AssetCollectorGroupData> Groups = new();

        public IEnumerable<AssetAlias> GetAssets()
        {
            return Groups.SelectMany(group => group.GetAssets());
        }
    }

    [CreateAssetMenu(menuName = "ybwork/Assets/AssetCollectorData")]
    internal class AssetCollectorData : ScriptableObject
    {
        public BuildTarget BuildTarget = BuildTarget.StandaloneWindows64;
        public string TargetPath;
        public List<AssetCollectorPackageData> Packages = new();

        public Dictionary<string, IEnumerable<AssetAlias>> GetAssets()
        {
            return Packages.ToDictionary(
                packages => packages.PackageName,
                packages => packages.GetAssets());
        }
    }
}
