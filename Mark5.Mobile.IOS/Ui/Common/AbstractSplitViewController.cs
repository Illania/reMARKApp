using System.Linq;
using UIKit;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public abstract class AbstractSplitViewController : UISplitViewController, ITaggedViewController
    {
        public string Tag { get; set; }

        protected AbstractSplitViewController()
        {
            SeparateSecondaryViewController = HandleSeparateSecondaryViewController;
            CollapseSecondViewController = HandleCollapseSecondViewController;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            CommonConfig.UsageAnalytics.SetScreen(GetType().Name);
        }

        public override void LoadView()
        {
            base.LoadView();

            PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;

            ViewControllers = new UIViewController[]
            {
                CreatePrimaryNavigationController(),
                CreateSecondaryNavigationController()
            };
        }

        protected abstract NavigationController CreatePrimaryNavigationController();

        protected abstract NavigationController CreateSecondaryNavigationController();

        UIViewController HandleSeparateSecondaryViewController(UISplitViewController splitViewController, UIViewController primaryViewController)
        {
            var primaryNavigationController = (NavigationController)primaryViewController;
            var lastPrimaryViewController = primaryNavigationController.ViewControllers.LastOrDefault(vc => vc is IPrimaryViewController);

            if (lastPrimaryViewController == null)
                return CreateSecondaryNavigationController();

            primaryNavigationController.ToolbarHidden = Integration.IsIPadOrMac();

            var poppedViewControllers = primaryNavigationController.PopToViewController(lastPrimaryViewController, false);
            if (poppedViewControllers != null && poppedViewControllers.Length > 0)
            {
                var secondaryNavigationController = new NavigationController();
                foreach (var poppedViewController in poppedViewControllers)
                    secondaryNavigationController.PushViewController(poppedViewController, false);

                return secondaryNavigationController;
            }

            return CreateSecondaryNavigationController();
        }

        bool HandleCollapseSecondViewController(UISplitViewController splitViewController, UIViewController secondaryViewController, UIViewController primaryViewController)
        {
            var secondaryNavigationController = (NavigationController)secondaryViewController;
            if ((secondaryNavigationController.ViewControllers[0] as ISecondaryViewController)?.Empty == true)
                return true;

            var primaryNavigationController = (NavigationController)primaryViewController;
            secondaryNavigationController.ViewControllers.ForEach(vc => primaryNavigationController.PushViewController(vc, false));

            return true;
        }
    }
}