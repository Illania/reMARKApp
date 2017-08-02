using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Utilities
{
    public class Worker
    {
        CancellationTokenSource cts;

        Func<Task> autoSaveAction;
        int delay;
        Task task;

        public Worker(Func<Task> autoSaveAction, int delay)
        {
            this.autoSaveAction = autoSaveAction;
            this.delay = delay;
        }

        public void Start()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            var ct = cts.Token;

            task = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(delay, ct);
                    }
                    catch (OperationCanceledException) { }

                    if (ct.IsCancellationRequested)
                        return;

                    await autoSaveAction();
                }
            });
        }

        public void Stop()
        {
            cts?.Cancel();
            cts = null;
        }

        public async Task Finished()
        {
            if (task == null)
                return;

            await Task.WhenAny(task);
        }
    }
}