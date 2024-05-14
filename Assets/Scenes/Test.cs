using UnityEngine;
using ybwork.Assets;

public class Test : MonoBehaviour
{
    async void Start()
    {
#if UNITY_EDITOR
        await AssetMgr.InitAsync_Editor(Application.streamingAssetsPath + "/Bundles/alias.json");
#else
        await AssetMgr.InitAsync_Release("D:\\Tao\\Desktop\\Coding\\Unity\\ybwork.YueAssets\\Bundles");
#endif
        AssetMgr.SetDefaultPackage("DefaultPackage");
        Instantiate(AssetMgr.LoadAssetSync<GameObject>("AA_Cube"));

        Instantiate(AssetMgr.GetPackage("aa").LoadAssetSync<GameObject>("Capsule"));
    }
}
