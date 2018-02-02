using System;
using Android.App.Job;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Utilities
{
    public class SystemConfigJobService : JobService
    {
        public override bool OnStartJob(JobParameters @params)
        {
            ServerConfig.SystemSettings = Managers.SystemManager.GetSystemSettingsAsync(SourceType.Remote).Result;
            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            return true;
        }
    }
}
