using System;
using System.Diagnostics;
using Foundation;
using LocalAuthentication;
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities.Fingerprint
{
    public class FingerprintAuthUtilities
    {
        public static Stopwatch Stopwatch;

        public static bool AuthenticationRequired
        {
            get 
            {
                return PlatformConfig.Preferences.FingerprintInterval != -1 && Stopwatch.Elapsed.Minutes >= PlatformConfig.Preferences.FingerprintInterval;
            }
        }

        static FingerprintAuthUtilities()
        {
            Stopwatch = new Stopwatch();
        }

        public static void Authenticate(UIApplication application)
        {
            if(AuthenticationRequired)
            {
                if(Stopwatch.IsRunning)
                    Stopwatch.Stop();
              
                var laContext = new LAContext();
                var authReason = new NSString("To continue using the app");

                var replyHandler = new LAContextReplyHandler((bool success, NSError error) =>
                {
                    application.InvokeOnMainThread(() =>
                    {
                        if (success)
                        {
                            CommonConfig.Logger.Info("Local Authentication succeeded.");
                            Stopwatch.Reset();
                        }
                    });
                });
                
                laContext.EvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, authReason, replyHandler);
            }
        }
    }
}