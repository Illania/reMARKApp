using Mark5.Mobile.Droid.Service;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid
{
    public static class PlatformConfig
    {
        public static SSLCertificateVerificationManager SSLCertificateVerificationManager { get; set; }
        public static ReachabilityBroadcastReceiver ReachabilityBroadcastReceiver { get; set; }
        public static CallStateBroadcastReceiver CallStateBroadcastReceiver { get; set; }
        public static Preferences Preferences { get; set; }
    }
}