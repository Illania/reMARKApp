using System;
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

        static int jobId;
        static ComponentName serviceComponent = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(SystemSettingsJobService)));

        public static void ScheduleJob()
        {
            //Runs at some point in the interval if there is a network connection, otherwise at the end of the interval. 
            var builder = new JobInfo.Builder(jobId++, serviceComponent);
            builder.SetRequiredNetworkType(NetworkType.Any);
            builder.SetPeriodic(OneDayInterval);
            var jobInfo = builder.Build();

            var jobScheduler = (JobScheduler) Application.Context.GetSystemService(JobSchedulerService);
            jobScheduler.Schedule(jobInfo);
        }

        public override bool OnStartJob(JobParameters parameters)
        {
            Task.Run(async () =>
            {
                try
                {
                    CommonConfig.Logger.Info("SystemConfigJobService: Retrieving system settings...");

                    ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Remote);
                }
                catch(Exception ex)
                {
                    CommonConfig.Logger.Info(ex.Message);
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