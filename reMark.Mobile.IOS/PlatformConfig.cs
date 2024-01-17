using reMark.Mobile.IOS.Service;
using reMark.Mobile.IOS.Utilities;
using Preferences = reMark.Mobile.IOS.Utilities.Preferences;

namespace reMark.Mobile.IOS
{
    public static class PlatformConfig
    {
        public static SSLCertificateVerificationManager SSLCertificateVerificationManager { get; set; }
        public static Preferences Preferences { get; set; }
        public static ReachabilityReceiver ReachabilityReceiver { get; set; }
    }
}