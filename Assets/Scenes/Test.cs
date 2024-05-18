using UnityEngine;
using ybwork.Assets;

public class Test : MonoBehaviour
{
    IAsyncHandler _initHandler;

    async void Start()
    {
        //#if UNITY_EDITOR
        //_initHandler = AssetMgr.InitAsync_Editor();
        //#else
        _initHandler = AssetMgr.InitAsync_Release("https://ybwork.cn:12353/Bundles");
        //_initHandler = AssetMgr.InitAsync_Release(Environment.CurrentDirectory + "/YueAssets");
        //#endif

        await _initHandler.Task;

        AssetMgr.SetDefaultPackage("DefaultPackage");

        //Instantiate(AssetMgr.LoadAssetSync<GameObject>("AA_Cube"));
        //Instantiate(AssetMgr.GetPackage("aa").LoadAssetSync<GameObject>("Capsule"));
    }

    private void Update()
    {
        Debug.Log(_initHandler.Progress);
    }
}
