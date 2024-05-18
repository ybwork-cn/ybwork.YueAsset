using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ybwork.Assets
{
    internal class AssetBundleDownloadHandler : IAsyncDownloadHandler
    {
        long IAsyncDownloadHandler.DownloadedBytes => (long)_asyncOperation.webRequest.downloadedBytes;
        public long Length { get; }
        public IEnumerator Task { get; }
        public bool Completed { get; private set; } = false;

        public AssetBundle AssetBundle { get; private set; }

        public readonly string BundleName;

        private readonly UnityWebRequestAsyncOperation _asyncOperation;
        private readonly bool _cache;
        private readonly string _cachePath;
        protected Action _onComplete;

        public AssetBundleDownloadHandler(string url, string packageName, string bundleName, long length, bool cache)
        {
            Length = length;
            BundleName = bundleName;
            bundleName = bundleName.ToLower() + ".ab";

            _cache = cache;
            _cachePath = Path.Combine(AssetMgr.PersistentDataPath, packageName, bundleName);

            string webPath = Path.Combine(url, packageName, bundleName);

            UnityWebRequest request;
            if (cache)
            {
                request = UnityWebRequest.Get(webPath);
                DownloadHandlerFile handler = new DownloadHandlerFile(_cachePath);
                request.downloadHandler = handler;
                request.disposeDownloadHandlerOnDispose = true;
            }
            else
            {
                request = UnityWebRequestAssetBundle.GetAssetBundle(webPath);
            }
            _asyncOperation = request.SendWebRequest();
            Task = DownLoad();
        }

        public void Then(Action action)
        {
            if (Completed)
                action?.Invoke();
            else
                _onComplete += action;
        }

        private IEnumerator DownLoad()
        {
            yield return _asyncOperation;

            if (_cache)
            {
                var request = AssetBundle.LoadFromFileAsync(_cachePath);
                yield return request;
                AssetBundle = request.assetBundle;
            }
            else
            {
                AssetBundle = DownloadHandlerAssetBundle.GetContent(_asyncOperation.webRequest);
            }
            _onComplete?.Invoke();
        }
    }
}
