using System;
using UnityEngine.Networking;

namespace ybwork.Assets
{
    internal class DownloadHandler : IAsyncHandlerDownload
    {
        AsyncEvent IAsyncHandler.OnComplete { get; } = new();
        float IAsyncHandlerDownload.Progress => _unityWebRequest.downloadProgress;
        bool IAsyncHandler.Completed => _unityWebRequest.isDone;

        public string ContentText => _unityWebRequest.downloadHandler.text;
        public byte[] ContentData => _unityWebRequest.downloadHandler.data;

        private readonly UnityWebRequest _unityWebRequest;

        public DownloadHandler(string url)
        {
            _unityWebRequest = UnityWebRequest.Get(url);
            var webRequestAsyncOperation = _unityWebRequest.SendWebRequest();
            webRequestAsyncOperation.completed += (_) =>
            {
                this.OnCompleted();
            };
        }

        public void Then(Action<DownloadHandler> action)
        {
            this.Then(() => action?.Invoke(this));
        }
    }
}
