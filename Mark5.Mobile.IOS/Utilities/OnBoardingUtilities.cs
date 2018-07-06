using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers;
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
                string html = "";

                try
                {
                    //TODO: Add a proper changelog.
                    html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/changelogs/changelog_" + currentVersionCode, "html"));
                }
                catch (ArgumentNullException ex)
                {
                    CommonConfig.Logger.Error("There is no changelog for this version code!", ex);
                    return;
                }

                var pvc = new OnBoardingViewController
                {
                    ChangelogHtml = html
                };
                pvc.ModalPresentationStyle = UIModalPresentationStyle.Custom;

                viewController.PresentViewController(pvc, true, null);
            }
        }

        bool ApplicationHasBeenUpdated()
        {
            var storedVersionCode = userDefaults.IntForKey(appVersionKey);

            return currentVersionCode > storedVersionCode;
        }
    }
}