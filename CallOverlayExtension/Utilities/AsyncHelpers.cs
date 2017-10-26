using System;
using System.Threading.Tasks;
using Foundation;

namespace CallOverlayExtension.Utilities
{
    public static class AsyncHelpers
    {
        public static Task InvokeOnMainThreadAsync(NSObject obj, Func<Task> f)
        {
            if (NSThread.IsMain)
            {
                return f();
            }

            var tcs = new TaskCompletionSource<bool>();

            obj.BeginInvokeOnMainThread(async () =>
            {
                await f();
                tcs.SetResult(true);
            });

            return tcs.Task;
        }

        public static Task<T> InvokeOnMainThreadAsync<T>(NSObject obj, Func<Task<T>> f)
        {
            if (NSThread.IsMain)
            {
                return f();
            }

            var tcs = new TaskCompletionSource<T>();

            obj.BeginInvokeOnMainThread(async () =>
            {
                var result = await f();
                tcs.SetResult(result);
            });

            return tcs.Task;
        }
    }
}