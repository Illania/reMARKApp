using Mark5.Mobile.Droid.Service;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Droid.Utilities.DeviceReminder;

namespace Mark5.Mobile.Droid
{
    public static class PlatformConfig
    {
        public static SSLCertificateVerificationManager SSLCertificateVerificationManager { get; set; }
        public static CallStateBroadcastReceiver CallStateBroadcastReceiver { get; set; }
        public static DeviceReminderBroadcastReceiver DeviceReminderBroadcastReceiver { get; set; }
        public static ReachabilityMonitor ReachabilityMonitor { get; set; }
        public static Preferences Preferences { get; set; }
    }
}