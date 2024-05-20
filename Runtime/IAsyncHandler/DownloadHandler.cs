using System;
using System.Collections;
using UnityEngine.Networking;

namespace ybwork.Assets
{
    internal class DownloadHandler : IAsyncDownloadHandler
    {
        long IAsyncDownloadHandler.DownloadedBytes => (long)_downloadRequest.downloadedBytes;
        long IAsyncDownloadHandler.Length
        {
            get
            {
                if (_length == 0)
                {
                    string contentLength = _downloadRequest.GetResponseHeader("Content-Length");
                    long.TryParse(contentLength, out _length);
                }
                return _length;
            }
        }
        public IEnumerator Task { get; }
        public bool Completed { get; private set; } = false;

        private long _length = 0;
        public string ContentText => _downloadRequest.downloadHandler.text;
        public byte[] ContentData => _downloadRequest.downloadHandler.data;
        protected Action _onComplete;

        private readonly UnityWebRequest _downloadRequest;

        public DownloadHandler(string url)
        {
            _downloadRequest = UnityWebRequest.Get(url);
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
            Completed = true;
            _onComplete?.Invoke();
        }
    }
}
