using System;
using System.IO;
using Foundation;
using Mark5.Mobile.IOS.Ui.ViewControllers;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class OnBoardingUtilities
    {
        const string appVersionKey = "latestAppVersionKey";

        public static void ShowOnBoardingIfNecessary(UIViewController viewController)
        {
            if (ApplicationHasBeenUpdated())
            {
                SaveAppVersionCode();

                var currentVersionCode = Int32.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")).ToString());
                var changelogPath = NSBundle.MainBundle.PathForResource("html/changelogs/changelog_" + currentVersionCode, "html");
                var html = "";

                //TODO: Add a proper changelog.
                if (!File.Exists(changelogPath))
                    return;
                
                html = File.ReadAllText(changelogPath);

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

        static void SaveAppVersionCode()
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            var currentVersionCode = Int32.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")).ToString());
            userDefaults.SetInt(currentVersionCode, appVersionKey);
            userDefaults.Synchronize();
        }
    }
}