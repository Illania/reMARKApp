using System;
using Android.Content;
using AndroidX.Work;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Job;

namespace Mark5.Mobile.Droid.Utilities.Workers
{
    public class DeviceReminderWorker : Worker
    {
        public DeviceReminderWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
        {
        }

        public override Result DoWork()
        {
            try
            {
                AsyncHelpers.RunSync(Jobs.RemindersUpdateJob.Run);
                return new Result.Success();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while updating device reminders", ex);
                return new Result.Failure();
            }
        }

        public static void Schedule()
        {
            try
            {
                var constraints = new Constraints.Builder().SetRequiredNetworkType(NetworkType.Connected).Build();
                var pwr = PeriodicWorkRequest.Builder.From<DeviceReminderWorker>(1, Java.Util.Concurrent.TimeUnit.Hours)
                    .SetConstraints(constraints)
                    .Build();

                WorkManager.Instance.Enqueue(pwr);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while scheduling device reminder worker", ex);
            }
        }
    }
}
