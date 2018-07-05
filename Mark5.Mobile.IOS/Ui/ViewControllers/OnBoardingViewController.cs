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
        UIView mainView;
        UITextView topTextView;
        UIButton okButton;


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            mainView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Alpha = 1f
            };

            View.Add(mainView);

            View.AddConstraints(new[]
            {
                mainView.HeightAnchor.ConstraintEqualTo(View.HeightAnchor),
                mainView.WidthAnchor.ConstraintEqualTo(View.WidthAnchor),
                mainView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor)
            });

            topTextView = new UITextView
            {
                Text = "WEijfowejiafwijfaewjifaewifaweijoafweiojaewfijoæfwaæifaæijowifaewiæoefw",//Localization.GetString("whats_new"),
                TranslatesAutoresizingMaskIntoConstraints = false,
            };


            mainView.Add(topTextView);

            mainView.AddConstraints(new[]
            {
                topTextView.TopAnchor.ConstraintEqualTo(mainView.TopAnchor),
                topTextView.WidthAnchor.ConstraintEqualTo(mainView.WidthAnchor),
                topTextView.CenterXAnchor.ConstraintEqualTo(mainView.CenterXAnchor),
                topTextView.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor)
            });
                                    
            okButton = new UIButton
            {
                TintColor = Theme.LightGray,
                BackgroundColor = Theme.DarkBlue,
                ContentEdgeInsets = new UIEdgeInsets(12.5f, 40f, 12.5f, 40f),
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true
            };
            okButton.SetTitleColor(Theme.White, UIControlState.Normal);
            okButton.SetTitle(Localization.GetString("ok"), UIControlState.Normal);

            mainView.Add(okButton);

            mainView.AddConstraints(new[]
            {
                okButton.HeightAnchor.ConstraintEqualTo(65f),
                okButton.WidthAnchor.ConstraintEqualTo(mainView.WidthAnchor),
                okButton.CenterXAnchor.ConstraintEqualTo(mainView.CenterXAnchor),
                okButton.TopAnchor.ConstraintEqualTo(topTextView.BottomAnchor),
                okButton.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? mainView.SafeAreaLayoutGuide.BottomAnchor : BottomLayoutGuide.GetTopAnchor(), 2)
            });

            //View.BackgroundColor = UIColor.Black.ColorWithAlpha(0.3f);

            // spoView.LayoutIfNeeded();

        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            okButton.TouchUpInside += CancelButton_TouchUpInside;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            okButton.TouchUpInside -= CancelButton_TouchUpInside;
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

        public override CGRect FrameOfPresentedViewInContainerView => ContainerView.Bounds.Inset(-400, 400);

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