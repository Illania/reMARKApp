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
    public class FoldersNotificationsListViewController : AbstractViewController, IUIViewControllerRestoration
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
            
            segmentedControl = new UISegmentedControl(new[]
            {
                Localization.GetString("folders"),
                Localization.GetString("notifications")
            })
            {
                Frame = new CGRect(0f, 0f, 0f, 24f),
                SelectedSegment = 0
            };
            segmentedControl.AddTarget(this, new Selector("segmentedControlHasChangedValue:"), UIControlEvent.ValueChanged);

            NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Always;
            NavigationItem.Title = GetTitleForModule(moduleType);
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

            NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

            RestorationIdentifier = nameof(FoldersNotificationsListViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var vc = viewControllers[segmentedControl.SelectedSegment];
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

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                var ni = NavigationItem;

                if (ParentViewController != null && ParentViewController is UIViewController && !(ParentViewController is UINavigationController))
                    ni = ParentViewController?.NavigationItem;

                if (ni.SearchController == null)
                    ni.SearchController = currentViewController?.NavigationItem?.SearchController;
            });
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

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode((int)moduleType, "moduleType");
            coder.Encode(segmentedControl.SelectedSegment, "selectedSegment");
            coder.Encode(viewControllers[0], "vc_0");
            coder.Encode(viewControllers[1], "vc_1");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            segmentedControl.SelectedSegment = coder.DecodeInt("selectedSegment");
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            var moduleType = (ModuleType)coder.DecodeInt("moduleType");
            return new FoldersNotificationsListViewController(moduleType);
        }

        #endregion

    }
}