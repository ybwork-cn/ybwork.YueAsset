using UnityEngine.Networking;

namespace ybwork.Assets
{
    internal class DiskFileDownloadHandler : IAsyncHandlerDownload
    {
        AsyncEvent IAsyncHandler.OnComplete { get; } = new();
        float IAsyncHandlerDownload.Progress => _downloadRequest.downloadProgress;
        bool IAsyncHandler.Completed => _downloadRequest.isDone;

        private readonly UnityWebRequest _downloadRequest;

        public DiskFileDownloadHandler(string url, string filepath)
        {
            _downloadRequest = UnityWebRequest.Get(url);
            _downloadRequest.downloadHandler = new DownloadHandlerFile(filepath);
            var webRequestAsyncOperation = _downloadRequest.SendWebRequest();
            webRequestAsyncOperation.completed += (_) =>
            {
                this.OnCompleted();
            };
        }
    }
}
