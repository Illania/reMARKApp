using System.Threading;
using System.Threading.Tasks;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Classes;

namespace reMark.Mobile.Common.Service
{
    public abstract class AbstractService : IAbstractService
    {
        static readonly object lockObj = new object();

        Task workerTask;
        CancellationTokenSource workerTaskCts;

        SemaphoreSlim mainSemaphore = new SemaphoreSlim(0);

        private readonly int autoRunPeriod;

        protected AbstractService(int autoRunPeriod)
        {
            this.autoRunPeriod = autoRunPeriod;
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

                CommonConfig.Reachability.ReachabilityRefreshed -= ReachabilityRefreshed;
                CommonConfig.Reachability.ReachabilityRefreshed += ReachabilityRefreshed;

                if (!CommonConfig.Reachability.IsReachable)
                        return;
                               
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

                if (!allowRestart)
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