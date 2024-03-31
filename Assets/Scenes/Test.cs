using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ybwork.Assets;

public class Test : MonoBehaviour
{
    async void Start()
    {
        await AssetMgr.InitAsync();
        AssetMgr.SetDefaultPackage("DefaultPackage");
        Instantiate(AssetMgr.LoadAssetSync<GameObject>("New Prefab"));

        Instantiate(AssetMgr.GetPackage("aa").LoadAssetSync<GameObject>("Capsule"));
    }
}
