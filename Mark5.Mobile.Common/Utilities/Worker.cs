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

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(delay);
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
    }
}