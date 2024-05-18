using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace ybwork.Assets
{
    internal static class Extension
    {
        public static TaskAwaiter<T> GetAwaiter<T>(this T asyncOperation) where T : AsyncOperation
        {
            TaskCompletionSource<T> taskCompletionSource = new();
            asyncOperation.completed += (AsyncOperation obj) =>
            {
                taskCompletionSource.SetResult(obj as T);
            };
            return taskCompletionSource.Task.GetAwaiter();
        }

        public static ulong Sum<T>(this IEnumerable<T> enumerator, Func<T, ulong> func)
        {
            ulong result = 0;
            foreach (var item in enumerator)
            {
                result += func(item);
            }
            return result;
        }
    }
}
