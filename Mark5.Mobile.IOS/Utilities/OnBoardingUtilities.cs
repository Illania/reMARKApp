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

        public static void ShowOnBoardingIfNecessary(UIViewController viewController)
        {
            try  //TODO need to remove html
            {
                if (ApplicationHasBeenUpdated())
                {
                    SaveAppVersionCode();

                    var currentVersionName = float.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleShortVersionString")).ToString());

                    var pvc = new OnBoardingViewController
                    {
                    };
                    pvc.ModalPresentationStyle = UIModalPresentationStyle.Custom;

                    viewController.PresentViewController(pvc, true, null);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("An error occured when showing onboarding.", ex);
                return;
            }
        }

        static bool ApplicationHasBeenUpdated()
        {
            return true; //TODO testing

            var userDefaults = NSUserDefaults.StandardUserDefaults;
            var currentVersionName = float.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleShortVersionString")).ToString());
            var storedVersionName = userDefaults.FloatForKey(appVersionKey);

            return currentVersionName > storedVersionName;
        }

        static void SaveAppVersionCode()
        {
            var currentVersionName = float.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleShortVersionString")).ToString());
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            userDefaults.SetFloat(currentVersionName, appVersionKey);
            userDefaults.Synchronize();
        }
    }
}