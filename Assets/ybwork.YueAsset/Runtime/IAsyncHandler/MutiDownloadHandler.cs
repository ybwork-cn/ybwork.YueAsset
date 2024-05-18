using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ybwork.Assets
{
    internal class MutiDownloadHandler : IAsyncDownloadHandler
    {
        long IAsyncDownloadHandler.DownloadedBytes => _handlers.Sum(downloadHandler => downloadHandler.DownloadedBytes);
        long IAsyncDownloadHandler.Length => _length;
        public IEnumerator Task => _task;
        public bool Completed { get; private set; } = false;

        private readonly List<IAsyncDownloadHandler> _handlers = new();
        private readonly IEnumerator _task;
        private bool _started = false;
        private readonly IEnumerator _preTask;
        protected Action _onComplete;
        private long _length;

        public MutiDownloadHandler(IAsyncHandler preTask = null)
        {
            _preTask = preTask?.Task;
            _task = WhenAll();
        }

        public void Start()
        {
            _started = true;
            _length = _handlers.Sum(downloadHandler => downloadHandler.Length);
        }

        public void AddDependency(IAsyncDownloadHandler handler)
        {
            _handlers.Add(handler);
        }

        public void Then(Action action)
        {
            if (Completed)
                action?.Invoke();
            else
                _onComplete += action;
        }

        private IEnumerator WhenAll()
        {
            yield return _preTask;
            while (!_started)
                yield return null;
            foreach (IEnumerator item in _handlers.Select(handler => handler.Task))
            {
                yield return item;
            }
            _onComplete?.Invoke();
        }
    }
}
