using System;
using System.Collections;
using UnityEngine;
using ybwork.Assets;

public class Test : MonoBehaviour
{
    IAsyncHandler _initHandler;

    IEnumerator Start()
    {
#if UNITY_EDITOR
        _initHandler = AssetMgr.InitAsync_Editor();
        //_initHandler = AssetMgr.InitAsync_Editor(Environment.CurrentDirectory + "/YueAssets/alias.json");
#else
        //_initHandler = AssetMgr.InitAsync_Release("http://localhost:8080/");
        _initHandler = AssetMgr.InitAsync_Release(Environment.CurrentDirectory + "/YueAssets");
#endif

        yield return _initHandler.Task;

        AssetMgr.SetDefaultPackage("DefaultPackage");

        Instantiate(AssetMgr.LoadAssetSync<GameObject>("AA_Cube"));
        Instantiate(AssetMgr.GetPackage("aa").LoadAssetSync<GameObject>("Capsule"));
    }

    private void Update()
    {
        Debug.Log(Math.Round(_initHandler.Progress * 1000) / 10);
    }
}
