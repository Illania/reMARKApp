using System;
using System.Diagnostics;
using Foundation;
using LocalAuthentication;
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities.Fingerprint
{
    public class FingerprintAuthentication
    {
        public static Stopwatch Stopwatch;

        public static bool AuthenticationRequired
        {
            get
            {
                return PlatformConfig.Preferences.FingerprintInterval != -1 && Stopwatch.Elapsed.Minutes >= PlatformConfig.Preferences.FingerprintInterval;
            }
        }

        static FingerprintAuthentication()
        {
            Stopwatch = new Stopwatch();
        }

        public static void Authenticate()
        {
            if (AuthenticationRequired)
            {
                if (Stopwatch.IsRunning)
                    Stopwatch.Stop();

                var laContext = new LAContext();
                NSError policyError;
                var authReason = new NSString("To continue using the app");

                if (laContext.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out policyError))
                {
                    var replyHandler = new LAContextReplyHandler((bool success, NSError error) =>
                    {
                        if (success)
                        {
                            CommonConfig.Logger.Info("Local Authentication succeeded.");
                            Stopwatch.Reset();
                        }
                    });

                    laContext.EvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, authReason, replyHandler);
                }
                else 
                {
                    Stopwatch.Reset();

                    if (policyError != null)
                        CommonConfig.Logger.Info("Policy can't be evaluated: " + policyError.LocalizedDescription);
                }
            }
        }
    }
}