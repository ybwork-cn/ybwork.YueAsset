using System;
using System.Collections.Generic;

namespace ybwork.Assets
{
    internal class AsyncEvent
    {
        readonly Queue<Action> _actions = new();

        public void AddListener(Action action)
        {
            _actions.Enqueue(action);
        }

        public void RemoveAllListeners()
        {
            _actions.Clear();
        }

        public void Invoke()
        {
            while (_actions.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
        }
    }

    public interface IAsyncHandler
    {
        public bool Completed { get; }
        internal AsyncEvent OnComplete { get; }
    }

    public interface IAsyncHandlerDownload : IAsyncHandler
    {
        /// <summary>
        /// 返回[0,1]区间的数字，用于指示进度
        /// </summary>
        public float Progress { get; }
    }

    public static class AsyncHandlerExtension
    {
        public static void Then(this IAsyncHandler asyncHandler, Action action)
        {
            if (asyncHandler.Completed)
            {
                if (!UnityEngine.Application.isPlaying)
                    return;
                action?.Invoke();
            }
            else
            {
                asyncHandler.OnComplete.AddListener(action);
            }
        }

        internal static void OnCompleted(this IAsyncHandler asyncHandler)
        {
            if (!UnityEngine.Application.isPlaying)
                return;
            asyncHandler.OnComplete.Invoke();
            asyncHandler.OnComplete.RemoveAllListeners();
        }
    }
}
