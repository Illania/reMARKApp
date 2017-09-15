using System;
using System.Threading.Tasks;
using Foundation;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class AsyncHelpers
    {
        public static Task InvokeOnMainThreadAsync(NSObject obj, Func<Task> f)
        {
            var tcs = new TaskCompletionSource<bool>();

            obj.BeginInvokeOnMainThread(async () => {
                await f();
                tcs.SetResult(true);
            });

            return tcs.Task;
        }
    }
}