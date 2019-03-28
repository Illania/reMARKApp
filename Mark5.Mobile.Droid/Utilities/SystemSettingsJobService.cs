using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Content;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Utilities
{
    [Service(Permission = "android.permission.BIND_JOB_SERVICE")]
    class SystemSettingsJobService : JobService
    {
        static readonly long OneDayInterval = 1000 * 60 * 60 * 24;
        static readonly int JobId = 1; //Must be unique for all jobservices scheduled by reMARK.

        static readonly ComponentName serviceComponent = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(SystemSettingsJobService)));

        public static void ScheduleJob()
        {
            try
            {
                var jobScheduler = (JobScheduler)Application.Context.GetSystemService(JobSchedulerService);

                if (jobScheduler.AllPendingJobs.Any(j => (j.Id == JobId))) //Job already scheduled.
                    return;

                //Runs at some point in the interval if there is a network connection, otherwise at the end of the interval. 
                var builder = new JobInfo.Builder(JobId, serviceComponent);
                builder.SetRequiredNetworkType(NetworkType.Any);
                builder.SetPeriodic(OneDayInterval);
                var jobInfo = builder.Build();

                jobScheduler.Schedule(jobInfo);

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while scheduling system settings job", ex);
            }
        }

        public override bool OnStartJob(JobParameters parameters)
        {
            Task.Run(async () =>
            {
                try
                {
                    CommonConfig.Logger.Info("SystemSettingsJobService: Retrieving system settings...");

                    ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Remote);

                    if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                        await Managers.SystemManager.GetSystemUsersDepartmentsAsync(SourceType.Remote);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("SystemSettingsJobService: Error while retrieving system settings.", ex);
                }
                finally
                {
                    JobFinished(parameters, false);
                }
            });
            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            return false;
        }
    }
}