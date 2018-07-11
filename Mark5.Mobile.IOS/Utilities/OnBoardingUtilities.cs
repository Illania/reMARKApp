using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class OnBoardingUtilities
    {
        const string appVersionKey = "latestAppVersionKey";

        public static void SaveAppVersionCode()
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            userDefaults.SetInt(currentVersionCode, appVersionKey);
            userDefaults.Synchronize();
        }

        public static void TryShowingOnBoardingDialog(UIViewController viewController)
        {
            if (ApplicationHasBeenUpdated())
            {
                SaveAppVersionCode();

                var currentVersionCode = Int32.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")).ToString());
                string html = "";

                //TODO: Add a proper changelog.
                html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/changelogs/changelog_" + currentVersionCode, "html"));

                var pvc = new OnBoardingViewController
                {
                    ChangelogHtml = html
                };
                pvc.ModalPresentationStyle = UIModalPresentationStyle.Custom;

                viewController.PresentViewController(pvc, true, null);
            }
        }

        static bool ApplicationHasBeenUpdated()
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            var currentVersionCode = Int32.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")).ToString());
            var storedVersionCode = userDefaults.IntForKey(appVersionKey);

            return currentVersionCode > storedVersionCode;
        }
    }
}