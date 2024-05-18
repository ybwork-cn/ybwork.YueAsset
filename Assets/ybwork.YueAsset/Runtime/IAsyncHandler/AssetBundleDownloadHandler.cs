using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ybwork.Assets
{
    internal class DownloadHandler : IAsyncDownloadHandler
    {
        ulong IAsyncDownloadHandler.DownloadedBytes => _asyncOperation.webRequest.downloadedBytes;
        ulong IAsyncDownloadHandler.Length
        {
            get
            {
                if (_length == 0)
                {
                    string contentLength = _asyncOperation.webRequest.GetResponseHeader("Content-Length");
                    Debug.Log(contentLength);
                    ulong.TryParse(contentLength, out _length);
                }
                return _length;
            }
        }
        public Task Task { get; }

        private ulong _length = 0;
        public string ContentText => _asyncOperation.webRequest.downloadHandler.text;
        public byte[] ContentData => _asyncOperation.webRequest.downloadHandler.data;

        private readonly UnityWebRequestAsyncOperation _asyncOperation;

        public DownloadHandler(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            _asyncOperation = request.SendWebRequest();
            Task = DownLoad();
        }

        private Task DownLoad()
        {
            TaskCompletionSource<object> taskCompletion = new();
            _asyncOperation.completed += (_) => taskCompletion.SetResult(null);
            return taskCompletion.Task;
        }
    }

    internal class AssetBundleDownloadHandler : IAsyncDownloadHandler
    {
        ulong IAsyncDownloadHandler.DownloadedBytes => _asyncOperation.webRequest.downloadedBytes;
        ulong IAsyncDownloadHandler.Length
        {
            get
            {
                if (_length == 0)
                {
                    string contentLength = _asyncOperation.webRequest.GetResponseHeader("Content-Length");
                    Debug.Log(contentLength);
                    ulong.TryParse(contentLength, out _length);
                }
                return _length;
            }
        }
        Task IAsyncHandler.Task => Task;

        private ulong _length = 0;
        public readonly Task<AssetBundle> Task;
        public AssetBundle AssetBundle { get; private set; }

        public readonly string BundleName;

        private readonly UnityWebRequestAsyncOperation _asyncOperation;
        private readonly bool _cache;
        private readonly string _cachePath;

        public AssetBundleDownloadHandler(string url, string packageName, string bundleName, bool cache)
        {
            BundleName = bundleName;
            bundleName = bundleName.ToLower() + ".ab";

            _cache = cache;
            _cachePath = Path.Combine(AssetMgr.PersistentDataPath, packageName, bundleName);

            string webPath = Path.Combine(url, packageName, bundleName);

            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(webPath);
            if (cache)
            {
                DownloadHandlerFile handler = new DownloadHandlerFile(_cachePath);
                request.downloadHandler = handler;
                request.disposeDownloadHandlerOnDispose = true;
            }
            _asyncOperation = request.SendWebRequest();
            Task ??= DownLoad();
        }

        private async Task<AssetBundle> DownLoad()
        {
            TaskCompletionSource<object> taskCompletion = new();
            _asyncOperation.completed += (_) => taskCompletion.SetResult(null);
            await _asyncOperation;

            if (_cache)
            {
                AssetBundle = AssetBundle.LoadFromFile(_cachePath);
            }
            else
            {
                AssetBundle = DownloadHandlerAssetBundle.GetContent(_asyncOperation.webRequest);
            }
            return AssetBundle;
        }
    }
}
