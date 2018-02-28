using System;
using Foundation;
using LocalAuthentication;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities.Fingerprint
{
    public class FingerprintAuthUtilities
    {
        public static void Authenticate(UIApplication application)
        {
            var laContext = new LAContext();
            NSError authError;
            var authReason = new NSString("To continue using the app");

            var replyHandler = new LAContextReplyHandler((bool success, NSError error) =>
            {
                application.InvokeOnMainThread(() =>
                {
                    if (success)
                    {
                        Console.Write("Succeed");
                    }
                    else
                    {
                        Console.Write("FAIL!");
                        //Reuqest pin code
                    }
                });
            });
        }
    }
}
