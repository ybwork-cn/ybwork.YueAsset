using System;
using System.Collections;
using UnityEngine.Networking;

namespace ybwork.Assets
{
    internal class DownloadHandler : IAsyncDownloadHandler
    {
        long IAsyncDownloadHandler.DownloadedBytes => (long)_asyncOperation.webRequest.downloadedBytes;
        long IAsyncDownloadHandler.Length
        {
            get
            {
                if (_length == 0)
                {
                    string contentLength = _asyncOperation.webRequest.GetResponseHeader("Content-Length");
                    long.TryParse(contentLength, out _length);
                }
                return _length;
            }
        }
        public IEnumerator Task { get; }
        public bool Completed { get; private set; } = false;

        private long _length = 0;
        public string ContentText => _asyncOperation.webRequest.downloadHandler.text;
        public byte[] ContentData => _asyncOperation.webRequest.downloadHandler.data;
        protected Action _onComplete;

        private readonly UnityWebRequestAsyncOperation _asyncOperation;

        public DownloadHandler(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
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
            Completed = true;
            _onComplete?.Invoke();
        }
    }
}
