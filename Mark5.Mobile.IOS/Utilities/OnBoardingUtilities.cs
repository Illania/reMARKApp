using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers;
using Foundation;
using System;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public class OnBoardingUtilities
    {
        const string appVersionKey = "latestAppVersionKey";
        readonly int currentVersionCode;
        readonly NSUserDefaults userDefaults;

        public OnBoardingUtilities()
        {
            userDefaults = NSUserDefaults.StandardUserDefaults;
            currentVersionCode = Int32.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")).ToString());
        }

        public void SaveAppVersionCode()
        {
            userDefaults.SetInt(currentVersionCode, appVersionKey);
            userDefaults.Synchronize();
        }

        public void TryShowingOnBoardingDialog(UIViewController viewController)
        {
            if (ApplicationHasBeenUpdated())
            {
                SaveAppVersionCode();

                var pvc = new OnBoardingViewController
                {
                    VersionCode = currentVersionCode
                };
                pvc.ModalPresentationStyle = UIModalPresentationStyle.Custom;

                viewController.PresentViewController(pvc, true, null);
            }
        }

        bool ApplicationHasBeenUpdated()
        {
            var storedVersionCode = userDefaults.IntForKey(appVersionKey);

            return true;//currentVersionCode > storedVersionCode;
        }
    }
}