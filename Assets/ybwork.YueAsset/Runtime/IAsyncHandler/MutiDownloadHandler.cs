using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ybwork.Assets
{
    internal class MutiDownloadHandler : IAsyncDownloadHandler
    {
        ulong IAsyncDownloadHandler.DownloadedBytes => _handlers.Sum(downloadHandler => downloadHandler.DownloadedBytes);
        ulong IAsyncDownloadHandler.Length => _handlers.Sum(downloadHandler => downloadHandler.DownloadedBytes);
        Task IAsyncHandler.Task
        {
            get
            {
                if (_task == null)
                {
                    IEnumerable<Task> tasks = _handlers
                        .Select(handler => handler.Task)
                        .Append(_preTask);
                    _task = Task.WhenAll(tasks);
                }
                return _task;
            }
        }

        private readonly List<IAsyncDownloadHandler> _handlers = new();
        private Task _task;
        private readonly Task _preTask;

        public MutiDownloadHandler(IAsyncHandler preTask = null)
        {
            _preTask = preTask?.Task ?? Task.CompletedTask;
        }

        public void AddDependency(IAsyncDownloadHandler handler)
        {
            _handlers.Add(handler);
        }
    }
}
