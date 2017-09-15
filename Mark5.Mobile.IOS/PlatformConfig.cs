using Mark5.Mobile.IOS.Service;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS
{
    public static class PlatformConfig
    {
        public static SSLCertificateVerificationManager SSLCertificateVerificationManager { get; set; }
        public static Preferences Preferences { get; set; }
        public static ReachabilityReceiver ReachabilityReceiver { get; set; }
    }
}