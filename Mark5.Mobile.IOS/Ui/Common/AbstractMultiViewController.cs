using CoreGraphics;
using Foundation;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class AbstractMultiViewController : AbstractViewController
    {
        protected UISegmentedControl SegmentedControl;
        protected UIViewController[] ViewControllers;
        protected UIViewController CurrentViewController;

        public override void LoadView()
        {
            base.LoadView();

            SegmentedControl = new UISegmentedControl
            {
                Frame = new CGRect(0f, 0f, 0f, 24f)
            };
            SegmentedControl.AddTarget(this, new Selector("segmentedControlHasChangedValue:"), UIControlEvent.ValueChanged);

            //if (Integration.IsRunningAtLeast(13))
            //    SegmentedControl.SelectedSegmentTintColor = Theme.DarkBlue;  //TODO
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.TitleView = SegmentedControl;

            SegmentedControl.SelectedSegment = 0;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var vc = ViewControllers[SegmentedControl.SelectedSegment];
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
            CurrentViewController = vc;
        }

        protected override void Recycle()
        {
            base.Recycle();

            SegmentedControl.RemoveTarget(this, new Selector("segmentedControlHasChangedValue:"), UIControlEvent.ValueChanged);
            SegmentedControl = null;
            ViewControllers = null;
            CurrentViewController = null;
        }

        [Export("segmentedControlHasChangedValue:")]
        void SegmentedControlHasChangedValue(UISegmentedControl sender)
        {
            View.EndEditing(true);

            var vc = ViewControllers[sender.SelectedSegment];
            CurrentViewController.WillMoveToParentViewController(null);
            vc.WillMoveToParentViewController(this);
            CurrentViewController.RemoveFromParentViewController();
            AddChildViewController(vc);
            CurrentViewController.View.RemoveFromSuperview();
            vc.View.Frame = View.Bounds;
            View.AddSubview(vc.View);
            if (!Integration.IsRunningAtLeast(11))
                NavigationController.View.SetNeedsLayout();
            if (vc.NavigationItem.LeftBarButtonItems != null)
                NavigationItem.SetLeftBarButtonItems(vc.NavigationItem.LeftBarButtonItems, false);
            else
                NavigationItem.SetLeftBarButtonItem(null, false);
            if (vc.NavigationItem.RightBarButtonItems != null)
                NavigationItem.SetRightBarButtonItems(vc.NavigationItem.RightBarButtonItems, false);
            else
                NavigationItem.SetRightBarButtonItem(null, false);
            vc.DidMoveToParentViewController(this);
            CurrentViewController.DidMoveToParentViewController(null);
            CurrentViewController = vc;
        }
    }
}
