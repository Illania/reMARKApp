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
        readonly NSUserDefaults userDefaults;

        public OnBoardingUtilities()
        {
            userDefaults = NSUserDefaults.StandardUserDefaults;
        }

        public void SaveAppVersionCode()
        {
            var versionCode = Int32.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")).ToString());
            userDefaults.SetInt(versionCode, appVersionKey);
            userDefaults.Synchronize();
        }

        bool ApplicationHasBeenUpdated()
        {
            var storedVersion = userDefaults.IntForKey(appVersionKey);
            var currentVersion = Int32.Parse(NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")).ToString());

            return currentVersion > storedVersion;
        }

        public void TryShowingOnBoardingDialog(UIViewController vc)
        {
            //if(ApplicationHasBeenUpdated())
            //{
            SaveAppVersionCode();
           
            var pvc = new OnBoardingViewController();
            pvc.ModalPresentationStyle = UIModalPresentationStyle.Custom;
            var customPresentationController = new OnBoardingPresentationController(pvc,vc);
            pvc.TransitioningDelegate = customPresentationController;

            vc.PresentViewController(new NavigationController(pvc), true, null);
            //}
        }
    }
}