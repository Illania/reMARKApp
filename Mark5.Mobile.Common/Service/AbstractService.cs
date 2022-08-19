using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Manager;

namespace Mark5.Mobile.Common.Service
{
    public abstract class AbstractService : IAbstractService
    {
        static readonly object lockObj = new object();

        Task workerTask;
        CancellationTokenSource workerTaskCts;

        SemaphoreSlim mainSemaphore = new SemaphoreSlim(0);

        private readonly int autoRunPeriod;
        private readonly bool trackReachability;

        protected AbstractService(int autoRunPeriod, bool trackReachability = true)
        {
            this.autoRunPeriod = autoRunPeriod;
            this.trackReachability = trackReachability;
        }

        public bool IsRunning()
        {
            lock (lockObj)
                return workerTask != null;
        }

        public void Notify() => mainSemaphore.Release();

        public void Start()
        {
            lock (lockObj)
            {
                if (workerTask != null)
                    return;

                if(trackReachability)
                {
                    CommonConfig.Reachability.ReachabilityRefreshed -= ReachabilityRefreshed;
                    CommonConfig.Reachability.ReachabilityRefreshed += ReachabilityRefreshed;
                    if (!CommonConfig.Reachability.IsReachable)
                        return;
                }
               
                if (Managers.ActiveConnectionInfo == null)
                    return;

                workerTaskCts?.Cancel();
                workerTaskCts = new CancellationTokenSource();

                workerTask = Task.Run(async () => await Work(workerTaskCts.Token));
            }
        }

        public void Stop(bool allowRestart = false)
        {
            lock (lockObj)
            {
                workerTask = null;
                workerTaskCts?.Cancel();

                if (!allowRestart && trackReachability)
                    CommonConfig.Reachability.ReachabilityRefreshed -= ReachabilityRefreshed;
            }
        }

        protected async Task Wait(CancellationToken ct) => await mainSemaphore.WaitAsync(autoRunPeriod, ct);

        protected abstract Task Work(CancellationToken ct);

        void ReachabilityRefreshed(object sender, ReachabilityRefreshedEventArgs e)
        {
            if (!e.Changed)
                return;

            if (e.IsReachable)
                Start();
            else
                Stop(true);
        }
    }
}