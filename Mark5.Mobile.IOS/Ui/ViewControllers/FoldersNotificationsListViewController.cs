//
// Project: Mark5.Mobile.IOS
// File: FoldersNotificationsListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities.Extensions;
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

        public override void LoadView()
        {
            base.LoadView();

            AutomaticallyAdjustsScrollViewInsets = false;

            segmentedControl = new UISegmentedControl(new[] { Localization.GetString("folders"), Localization.GetString("notifications") });
            segmentedControl.SetTitleTextAttributes(new UITextAttributes { Font = Theme.DefaultFont.WithRelativeSize(-3f), TextColor = Theme.White }, UIControlState.Normal);
            segmentedControl.SetTitleTextAttributes(new UITextAttributes { Font = Theme.DefaultFont.WithRelativeSize(-3f), TextColor = Theme.White }, UIControlState.Selected);
            segmentedControl.TintColor = Theme.DarkBlue;
            segmentedControl.SelectedSegment = 0;
            segmentedControl.AddTarget(this, new Selector("segmentedControlHasChangedValue:"), UIControlEvent.ValueChanged);

            NavigationItem.Prompt = GetTitleForModule(moduleType);
            NavigationItem.TitleView = segmentedControl;

            viewControllers = new UIViewController[]
            {
                new BrowseFoldersListViewController(moduleType),
                new NotificationsListViewController(moduleType.ObjectTypes())
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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

            //scrollView.ContentInset = new UIEdgeInsets(ParentViewController.TopLayoutGuide.Length + NavigationController.NavigationBar.Frame.Height, 0f, ParentViewController.BottomLayoutGuide.Length, 0f);
            //scrollView.ScrollIndicatorInsets = new UIEdgeInsets(ParentViewController.TopLayoutGuide.Length + NavigationController.NavigationBar.Frame.Height, 0f, ParentViewController.BottomLayoutGuide.Length, 0f);
            //scrollView.LayoutIfNeeded();
        }

        static string GetTitleForModule(ModuleType moduleType)
        {
            switch (moduleType)
            {
                case ModuleType.Documents:
                    return Localization.GetString("documents");
                case ModuleType.Contacts:
                    return Localization.GetString("contacts");
                case ModuleType.Shortcodes:
                    return Localization.GetString("shortcodes");
                case ModuleType.Calendar:
                    return Localization.GetString("contacts");
                default:
                    return string.Empty;
            }
        }
    }
}
