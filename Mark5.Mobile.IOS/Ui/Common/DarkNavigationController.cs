using System;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class DarkNavigationController : NavigationController
    {
        public DarkNavigationController()
        {
        }

        public DarkNavigationController(UIViewController rootViewController) : base(rootViewController)
        {
        }

        public DarkNavigationController(UIViewController rootViewController, UIModalPresentationStyle style)
            : base(rootViewController, style)
        {
        }

        public DarkNavigationController(UIViewController rootViewController, UIModalPresentationStyle iPhoneStyle, UIModalPresentationStyle iPadStyle)
            : base(rootViewController, iPhoneStyle, iPadStyle)
        {
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
        }
    }
}