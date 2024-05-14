using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ybwork.Assets;

public class Test : MonoBehaviour
{
    void Start()
    {
        AssetMgr.InitSync();
        AssetMgr.SetDefaultPackage("DefaultPackage");
        Instantiate(AssetMgr.LoadAssetSync<GameObject>("AA_Cube"));

        Instantiate(AssetMgr.GetPackage("aa").LoadAssetSync<GameObject>("Capsule"));
    }
}
