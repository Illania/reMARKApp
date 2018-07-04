using System;
using CoreGraphics;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class OnBoardingViewController : AbstractViewController
    {
        UIButton cancelButton;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            cancelButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            cancelButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            cancelButton.SetTitle(Localization.GetString("cancel"), UIControlState.Normal);

            View.Add(cancelButton);


            View.AddConstraints(new[]
            {
                cancelButton.HeightAnchor.ConstraintEqualTo(65f),
                cancelButton.WidthAnchor.ConstraintEqualTo(80f),
                cancelButton.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                cancelButton.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : BottomLayoutGuide.GetTopAnchor(), 2),
            });
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            cancelButton.TouchUpInside += CancelButton_TouchUpInside;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            cancelButton.TouchUpInside -= CancelButton_TouchUpInside;
        }

        void CancelButton_TouchUpInside(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }
    }

    public class OnBoardingPresentationController : UIPresentationController, IUIViewControllerTransitioningDelegate
    {
        public OnBoardingPresentationController(UIViewController presentedViewController, UIViewController presentingViewController) : base(presentedViewController, presentingViewController)
        {
        }

        public override CGRect FrameOfPresentedViewInContainerView => new CGRect(0, 0, 0.8*ContainerView.Bounds.Width, 0.8*ContainerView.Bounds.Height);

        public override void ContainerViewWillLayoutSubviews()
        {
            base.ContainerViewWillLayoutSubviews();

            PresentedView.Frame = FrameOfPresentedViewInContainerView; 
        }

        [Export("presentationControllerForPresentedViewController:presentingViewController:sourceViewController:")]
        public UIPresentationController GetPresentationControllerForPresentedViewController(UIViewController presentedViewController, UIViewController presentingViewController, UIViewController sourceViewController)
        {
            return this;
        }
    }
}