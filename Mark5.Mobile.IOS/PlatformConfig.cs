using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.IOS.Services;
using Mark5.Mobile.IOS.Utilities;
using TinyMessenger;

namespace Mark5.Mobile.IOS
{
    public static class PlatformConfig
    {
        public const string HockeyId = "c81873e5ee604880bf15a59e957f4d79";

        public static SSLCertificateVerificationManager SSLCertificateVerificationManager { get; set; }

        public static Preferences Preferences { get; set; }

        public static ReachabilityReceiver ReachabilityReceiver { get; set; }

        public static ITinyMessengerHub MessengerHub { get; set; }
    }
}