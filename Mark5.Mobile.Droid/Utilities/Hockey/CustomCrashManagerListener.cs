using HockeyApp.Android;

namespace Mark5.Mobile.Droid.Utilities.Hockey
{
    public class CustomCrashManagerListener : CrashManagerListener
    {
        public override bool OnHandleAlertView()
        {
            return true;
        }

        public override bool ShouldAutoUploadCrashes()
        {
            return PlatformConfig.Preferences.EnableReporting;
        }

        public override string Description => SystemReportCollector.CreateLogCatReport();
    }
}