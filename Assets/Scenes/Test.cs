using System;
using UnityEngine;
using ybwork.Assets;

public class Test : MonoBehaviour
{
    IAsyncHandlerDownload _initHandler;

    void Start()
    {
#if UNITY_EDITOR
        //_initHandler = AssetMgr.InitAsync_Editor();
#else
        //_initHandler = AssetMgr.InitAsync_Release("http://localhost:8080/");
        //_initHandler = AssetMgr.InitAsync_Release(Environment.CurrentDirectory + "/YueAssets");
#endif
        _initHandler = AssetMgr.InitAsync_Release(Environment.CurrentDirectory + "/YueAssets");
        _initHandler.Then(() =>
        {
            AssetMgr.SetDefaultPackage("DefaultPackage");
            Instantiate(AssetMgr.LoadAssetSync<GameObject>("AA_Cube"));
            Instantiate(AssetMgr.GetPackage("aa").LoadAssetSync<GameObject>("Capsule"));

            Debug.Log("启动完成");
        });
    }

    private void Update()
    {
        Debug.Log(Math.Round(_initHandler.Progress * 1000) / 10);
    }
}
