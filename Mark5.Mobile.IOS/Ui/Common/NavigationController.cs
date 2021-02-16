using Foundation;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class NavigationController : UINavigationController, ITaggedViewController, IUINavigationControllerDelegate
    {
        public string Tag { get; set; }

        public NavigationController()
        {
        }

        public NavigationController(UIViewController rootViewController)
            : base(rootViewController)
        {
        }

        public NavigationController(UIViewController rootViewController, UIModalPresentationStyle style)
            : this(rootViewController)
        {
            ModalPresentationStyle = style;
        }

        public NavigationController(UIViewController rootViewController, UIModalPresentationStyle iPhoneStyle, UIModalPresentationStyle iPadStyle)
            : this(rootViewController)
        {
            ModalPresentationStyle = Integration.IsIPadOrMac() ? iPadStyle : iPhoneStyle;
        }

        public override void LoadView()
        {
            Delegate = this;
            base.LoadView();
            View.BackgroundColor = Theme.White;
        }

        [Export("navigationController:willShowViewController:animated:")]
        public void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            var tc = navigationController.TopViewController.GetTransitionCoordinator();

            if (tc == null)
                return;

            var del = UIApplication.SharedApplication?.Delegate as AppDelegate;
            var root = del?.Window?.RootViewController as AbstractMainViewController;

            if (root == null)
                return;

            tc.AnimateAlongsideTransitionInView(root.View, animationContext =>
            {
                var fromVc = animationContext.GetViewControllerForKey(UITransitionContext.FromViewControllerKey);
                var toVc = animationContext.GetViewControllerForKey(UITransitionContext.ToViewControllerKey);

                if (fromVc.HidesBottomBarWhenPushed && (SplitViewController == null || SplitViewController.Collapsed))
                {
                    root?.SetBottomNavigationButtonsHidden(false);
                    root?.SetBottomNavigationButtonsAlpha(1f);
                }
                if (toVc.HidesBottomBarWhenPushed && (SplitViewController == null || SplitViewController.Collapsed))
                    root?.SetBottomNavigationButtonsAlpha(0f);

            }, completionContext =>
            {
                var toVc = completionContext.GetViewControllerForKey(UITransitionContext.ToViewControllerKey);
                if (toVc.HidesBottomBarWhenPushed && (SplitViewController == null || SplitViewController.Collapsed))
                    root?.SetBottomNavigationButtonsHidden(true);
            });
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (IsBeingDismissed
                || IsMovingFromParentViewController)
            {
                foreach (var vc in ViewControllers)
                {
                    (vc as AbstractViewController)?.RecycleIfNeeded();
                    (vc as AbstractTableViewController)?.RecycleIfNeeded();
                }
            }
        }

    }
}