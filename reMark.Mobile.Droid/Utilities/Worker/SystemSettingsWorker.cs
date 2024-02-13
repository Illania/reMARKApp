using System;
using Android.Content;
using AndroidX.Work;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Job;

namespace reMark.Mobile.Droid.Utilities.Workers
{
    public class SystemSettingsWorker : Worker
    {
        static readonly string workerName = "SystemSettingsWorker";

        public SystemSettingsWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
        {
        }

        public override Result DoWork()
        {
            return new Result.Success(); //Temporarily

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
            return; //Temporarily

        }
    }
}
