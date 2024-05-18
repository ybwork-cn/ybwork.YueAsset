using System;

namespace ybwork.Assets
{
    internal static class AsyncHandlerExtension
    {
        public static async void Then(this IAsyncHandler asyncHandler, Action action)
        {
            if (!asyncHandler.Task.IsCompleted)
                await asyncHandler.Task;
            action?.Invoke();
        }

        public static async void Then<T>(this T asyncHandler, Action<T> action) where T : IAsyncHandler
        {
            if (!asyncHandler.Task.IsCompleted)
                await asyncHandler.Task;
            action?.Invoke(asyncHandler);
        }
    }
}
