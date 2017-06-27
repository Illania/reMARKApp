using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.Service
{
    public abstract class AbstractService : IAbstractService
    {
        static readonly object lockObj = new object();

        Task workerTask;
        CancellationTokenSource workerTaskCts;

        protected SemaphoreSlim MainSemaphore = new SemaphoreSlim(0);

        public bool IsRunning()
        {
            lock (lockObj)
                return workerTask != null;
        }

        public void Notify() => MainSemaphore.Release();

        public void Start()
        {
            lock (lockObj)
            {
                CommonConfig.Logger.Info("Starting...");

                if (workerTask != null)
                    return;

                if (!CommonConfig.Reachability.IsReachable)
                    return;

                workerTaskCts?.Cancel();
                workerTaskCts = new CancellationTokenSource();

                CommonConfig.Reachability.ReachabilityRefreshed -= ReachabilityRefreshed;
                CommonConfig.Reachability.ReachabilityRefreshed += ReachabilityRefreshed;

                workerTask = Task.Run(async () => await Work(workerTaskCts.Token));

                CommonConfig.Logger.Info("Started");
            }
        }

        public void Stop(bool allowRestart = false)
        {
            lock (lockObj)
            {
                CommonConfig.Logger.Info("Stopping...");

                workerTask = null;
                workerTaskCts?.Cancel();

                if (!allowRestart)
                    CommonConfig.Reachability.ReachabilityRefreshed -= ReachabilityRefreshed;

                CommonConfig.Logger.Info("Stopped");
            }
        }

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