//
// Project: Mark5.Mobile.IOS
// File: AbstractSplitViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using UIKit;
using Mark5.Mobile.Common.Extensions;

namespace Mark5.Mobile.IOS.Ui.Common
{

    public abstract class AbstractSplitViewController : UISplitViewController, ITaggedViewController
    {
        
        public string Tag { get; set; }

        protected AbstractSplitViewController()
        {
            PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;

            SeparateSecondaryViewController = HandleSeparateSecondaryViewController;
            CollapseSecondViewController = HandleCollapseSecondViewController;
        }

        public override void LoadView()
        {
            base.LoadView();

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
            var primaryNavigationController = ((NavigationController)primaryViewController);
            var lastPrimaryViewController = primaryNavigationController.ViewControllers.LastOrDefault(vc => vc is IPrimaryViewController);

            if (lastPrimaryViewController == null) return CreateSecondaryNavigationController();

            var poppedViewControllers = primaryNavigationController.PopToViewController(lastPrimaryViewController, false);
            if (poppedViewControllers != null && poppedViewControllers.Length > 0)
            {
                var secondaryNavigationController = new NavigationController();
                foreach (var poppedViewController in poppedViewControllers)
                {
                    secondaryNavigationController.PushViewController(poppedViewController, false);
                }
                return secondaryNavigationController;
            }

            return CreateSecondaryNavigationController();
        }

        bool HandleCollapseSecondViewController(UISplitViewController splitViewController, UIViewController secondaryViewController, UIViewController primaryViewController)
        {
            var secondaryNavigationController = (NavigationController)secondaryViewController;
            if (((ISecondaryViewController)secondaryNavigationController.ViewControllers[0]).Empty) return true;

            var primaryNavigationController = (NavigationController)primaryViewController;
            secondaryNavigationController.ViewControllers.ForEach(vc => primaryNavigationController.PushViewController(vc, false));

            return true;
        }
    }
}
