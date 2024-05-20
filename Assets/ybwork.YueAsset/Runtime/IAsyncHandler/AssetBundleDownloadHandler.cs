using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ybwork.Assets
{
    internal class AssetBundleDownloadHandler : IAsyncDownloadHandler
    {
        long IAsyncDownloadHandler.DownloadedBytes => (long)(Math.Max(_downloadRequest.downloadProgress, 0) * Length);

        public long Length { get; }
        public IEnumerator Task { get; }
        public bool Completed { get; private set; } = false;

        public AssetBundle AssetBundle { get; private set; }

        public readonly string BundleName;

        private readonly UnityWebRequest _downloadRequest;
        protected Action _onComplete;

        public AssetBundleDownloadHandler(string url, string packageName, string bundleName, BundleGroupInfo bundleGroupInfo)
        {
            Length = bundleGroupInfo.Size;
            Hash128 hash = Hash128.Parse(bundleGroupInfo.Hash);
            BundleName = bundleName;

            string webPath = Path.Combine(url, packageName, bundleName);

            _downloadRequest = UnityWebRequestAssetBundle.GetAssetBundle(webPath, hash);

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
            yield return _downloadRequest.SendWebRequest();

            AssetBundle = DownloadHandlerAssetBundle.GetContent(_downloadRequest);
            _onComplete?.Invoke();
        }
    }
}
