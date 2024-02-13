using reMark.Mobile.Droid.Service;
using reMark.Mobile.Droid.Utilities;
using Preferences = reMark.Mobile.Droid.Utilities.Preferences;

namespace reMark.Mobile.Droid
{
    public static class PlatformConfig
    {
        public static SSLCertificateVerificationManager SSLCertificateVerificationManager { get; set; }
        public static CallStateBroadcastReceiver CallStateBroadcastReceiver { get; set; }
        public static ReachabilityMonitor ReachabilityMonitor { get; set; }
        public static Preferences Preferences { get; set; }
    }
}