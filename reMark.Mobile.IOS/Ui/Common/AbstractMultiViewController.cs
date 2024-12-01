using CoreGraphics;
using Foundation;
using reMark.Mobile.IOS.Utilities;
using ObjCRuntime;
using reMark.Mobile.Common;
using reMark.Mobile.IOS.Model;
using reMark.Mobile.IOS.Model.HubMessages;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public class AbstractMultiViewController : AbstractViewController
    {
        protected UISegmentedControl SegmentedControl;
        protected UIViewController[] ViewControllers;
        protected UIViewController CurrentViewController;

        private ToggleSwitchBarButtonItem toggleSwitch;

        public bool HasToggleBar {get; set;}

        public override void LoadView()
        {
            base.LoadView();

            SegmentedControl = new UISegmentedControl
            {
                Frame = new CGRect(0f, 0f, 0f, 24f)
            };
            SegmentedControl.AddTarget(this, new Selector("segmentedControlHasChangedValue:"), UIControlEvent.ValueChanged);

            if (Integration.IsRunningAtLeast(13))
                SegmentedControl.SelectedSegmentTintColor = Theme.DarkBlue;

            if (!HasToggleBar)
                return;

            toggleSwitch = new ToggleSwitchBarButtonItem();
            toggleSwitch.ToggleSwitchValueChanged += (sender, isOn) =>
            {
                SegmentedControl.SelectedSegment = isOn ? 1 : 0;
                HandleSelectedSegmentChanged((int)SegmentedControl.SelectedSegment);
            };
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

            if (HasToggleBar)
            {
                CreateRightBarButtonsWithToggle(vc);
            }
            else 
            {
                if (vc.NavigationItem.RightBarButtonItems != null)
                    NavigationItem.SetRightBarButtonItems(vc.NavigationItem.RightBarButtonItems, false);
                else
                    NavigationItem.SetRightBarButtonItem(null, false);
            }

            vc.DidMoveToParentViewController(this);
            CurrentViewController = vc;
        }

        public void CreateRightBarButtonsWithToggle(UIViewController vc)
        {
            var rightBarButtonItems = new List<UIBarButtonItem>();

            if (vc.NavigationItem.RightBarButtonItems != null)
                rightBarButtonItems.AddRange(vc.NavigationItem.RightBarButtonItems);
           
            rightBarButtonItems.AddRange(new List<UIBarButtonItem>{toggleSwitch.CreateToggleBarButtonItem()});
            NavigationItem.SetRightBarButtonItems(rightBarButtonItems.ToArray(), false);
        }

        protected override void Recycle()
        {
            base.Recycle();

            SegmentedControl.RemoveTarget(this, new Selector("segmentedControlHasChangedValue:"), UIControlEvent.ValueChanged);
            SegmentedControl = null;
            ViewControllers = null;
            CurrentViewController = null;
        }

        private void HandleSelectedSegmentChanged(int selectedSegment)
        {
            View.EndEditing(true);

            var vc = ViewControllers[selectedSegment];
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

            if (HasToggleBar)
            {
                CreateRightBarButtonsWithToggle(vc);       
            }
            else 
            {
                if (vc.NavigationItem.RightBarButtonItems != null)
                    NavigationItem.SetRightBarButtonItems(vc.NavigationItem.RightBarButtonItems, false);
                else
                    NavigationItem.SetRightBarButtonItem(null, false);
            }

            vc.DidMoveToParentViewController(this);
            CurrentViewController.DidMoveToParentViewController(null);
            CurrentViewController = vc;
        }

        [Export("segmentedControlHasChangedValue:")]
        public virtual void SegmentedControlHasChangedValue(UISegmentedControl sender)
        {
            View.EndEditing(true);

            var vc = ViewControllers[sender.SelectedSegment];
            if (CurrentViewController == null) 
                return;
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
