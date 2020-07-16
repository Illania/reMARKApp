using System;
using Android.Content;
using AndroidX.Work;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Job;

namespace Mark5.Mobile.Droid.Utilities.Workers
{
    public class SystemSettingsWorker: Worker
    {

        public SystemSettingsWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
        {
        }

        public override Result DoWork()
        {          
            try
            {
                AsyncHelpers.RunSync(Jobs.SystemSettingsUpdateJob.Run);
                return new Result.Success();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while updating system settings", ex);
                return new Result.Failure();
            }
        }

        public static void Schedule()
        {
            try
            {
                var constraints = new Constraints.Builder().SetRequiredNetworkType(NetworkType.Connected).Build();
                var pwr = PeriodicWorkRequest.Builder.From<DeviceReminderWorker>(1, Java.Util.Concurrent.TimeUnit.Days)
                    .SetConstraints(constraints)
                    .Build();
                
                WorkManager.Instance.Enqueue(pwr);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while scheduling system settings worker", ex);
            }

        }
    }
}
