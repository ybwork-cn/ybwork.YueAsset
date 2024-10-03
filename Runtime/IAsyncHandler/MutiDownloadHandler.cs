using System;
using System.Collections.Generic;
using System.Linq;

namespace ybwork.Assets
{
    internal class MutiDownloadHandler : IAsyncHandlerDownload
    {
        AsyncEvent IAsyncHandler.OnComplete { get; } = new();
        float IAsyncHandlerDownload.Progress => _handlers.Count == 0 ? 1 : _handlers.Sum(handler => handler.Progress) / _handlers.Count;
        public bool Completed { get; private set; } = false;

        private readonly List<IAsyncHandlerDownload> _handlers = new();

        private bool _isStarted = false;

        public MutiDownloadHandler() { }

        public void AddDependency(IAsyncHandlerDownload handler)
        {
            _handlers.Add(handler);
        }

        public void Start()
        {
            if (_isStarted)
                throw new NotImplementedException("不允许重复调用Start");

            _isStarted = true;
            foreach (IAsyncHandler handler in _handlers)
            {
                handler.Then(OnDependencyCompleted);
            }
        }

        private void OnDependencyCompleted()
        {
            if (Completed)
                return;
            int unCompeletedCount = _handlers.Count(h => !h.Completed);
            if (unCompeletedCount <= 0)
            {
                Completed = true;
                this.OnCompleted();
            }
        }
    }
}
