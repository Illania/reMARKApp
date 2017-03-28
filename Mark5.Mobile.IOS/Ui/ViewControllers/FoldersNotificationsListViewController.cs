//
// Project: Mark5.Mobile.IOS
// File: FoldersNotificationsListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class FoldersNotificationsListViewController : UIViewController
    {

        readonly ModuleType moduleType;

        UISegmentedControl segmentedControl;
        UIViewController[] viewControllers;

        UIViewController currentViewController;

        public FoldersNotificationsListViewController(ModuleType moduleType)
        {
            this.moduleType = moduleType;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AutomaticallyAdjustsScrollViewInsets = false;

            segmentedControl = new UISegmentedControl(new[] { Localization.GetString("folders"), Localization.GetString("notifications") });
            segmentedControl.SelectedSegment = 0;
            segmentedControl.AddTarget(this, new Selector("segmentedControlHasChangedValue:"), UIControlEvent.ValueChanged);
            NavigationItem.TitleView = segmentedControl;

            viewControllers = new UIViewController[]
            {
                new BrowseFoldersListViewController(moduleType),
                new NotificationsListViewController(moduleType)
            };

            var vc = viewControllers[0];
            vc.WillMoveToParentViewController(this);
            AddChildViewController(vc);
            vc.View.Frame = View.Bounds;
            View.AddSubview(vc.View);
            if (vc.NavigationItem.LeftBarButtonItems != null)
                NavigationItem.SetLeftBarButtonItems(vc.NavigationItem.LeftBarButtonItems, false);
            else
                NavigationItem.SetLeftBarButtonItem(null, false);
            if (vc.NavigationItem.RightBarButtonItems != null)
                NavigationItem.SetRightBarButtonItems(vc.NavigationItem.RightBarButtonItems, false);
            else
                NavigationItem.SetRightBarButtonItem(null, false);
            vc.DidMoveToParentViewController(this);
            currentViewController = vc;
        }

        [Export("segmentedControlHasChangedValue:")]
        void SegmentedControlHasChangedValue(UISegmentedControl sender)
        {
            var vc = viewControllers[sender.SelectedSegment];
            currentViewController.WillMoveToParentViewController(null);
            vc.WillMoveToParentViewController(this);
            currentViewController.RemoveFromParentViewController();
            AddChildViewController(vc);
            currentViewController.View.RemoveFromSuperview();
            vc.View.Frame = View.Bounds;
            View.AddSubview(vc.View);
            if (vc.NavigationItem.LeftBarButtonItems != null)
                NavigationItem.SetLeftBarButtonItems(vc.NavigationItem.LeftBarButtonItems, false);
            else
                NavigationItem.SetLeftBarButtonItem(null, false);
            if (vc.NavigationItem.RightBarButtonItems != null)
                NavigationItem.SetRightBarButtonItems(vc.NavigationItem.RightBarButtonItems, false);
            else
                NavigationItem.SetRightBarButtonItem(null, false);
            vc.DidMoveToParentViewController(this);
            currentViewController.DidMoveToParentViewController(null);
            currentViewController = vc;
            AdjustScrollViewInsets();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            AdjustScrollViewInsets();
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);

            coordinator.AnimateAlongsideTransition(ctx => { }, ctx => AdjustScrollViewInsets());
        }

        void AdjustScrollViewInsets()
        {
            var scrollView = currentViewController.View.Subviews[0] as UIScrollView;
            if (scrollView == null)
                return;

            scrollView.ContentInset = new UIEdgeInsets(ParentViewController.TopLayoutGuide.Length + NavigationController.NavigationBar.Frame.Height, 0f, ParentViewController.BottomLayoutGuide.Length, 0f);
            scrollView.ScrollIndicatorInsets = new UIEdgeInsets(ParentViewController.TopLayoutGuide.Length + NavigationController.NavigationBar.Frame.Height, 0f, ParentViewController.BottomLayoutGuide.Length, 0f);
            scrollView.LayoutIfNeeded();
        }
    }
}
