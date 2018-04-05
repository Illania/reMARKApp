using System.Diagnostics;
using Foundation;
using LocalAuthentication;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class LocalAuthenticationManager
    {
        static readonly Stopwatch stopwatch;

        static bool AuthenticationEnabled => PlatformConfig.Preferences.AuthorizationInterval != -1;

        static bool AuthenticationRequired => AuthenticationEnabled && stopwatch.Elapsed.Minutes >= PlatformConfig.Preferences.AuthorizationInterval;

        static bool authenticated;

        static LocalAuthenticationManager()
        {
            stopwatch = new Stopwatch();
        }

        public static void NotifyApplicationActivated()
        {
            if (AuthenticationRequired && !authenticated)
            {
                if (stopwatch.IsRunning)
                    stopwatch.Stop();

                var laContext = new LAContext();

                if (laContext.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out var policyError))
                {
                    var replyHandler = new LAContextReplyHandler((bool success, NSError error) =>
                    {
                        if (success)
                        {
                            authenticated = true;
                            CommonConfig.Logger.Info("Local Authentication succeeded.");
                            stopwatch.Reset();
                        }
                    });

                    laContext.LocalizedFallbackTitle = Localization.GetString("use_passcode");
                    laContext.EvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, Localization.GetString("localized_reason"), replyHandler);
                }
                else
                {
                    stopwatch.Reset();

                    if (policyError != null)
                        CommonConfig.Logger.Info("Policy can't be evaluated: " + policyError.LocalizedDescription);
                }
            }
        }

        public static void NotifyApplicationEnteredBackground()
        {
            authenticated = false;

            if (AuthenticationEnabled)
                stopwatch.Start();
        }

    }
}