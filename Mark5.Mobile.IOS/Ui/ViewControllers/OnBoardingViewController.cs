using System;
using CoreGraphics;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Foundation;
using UIKit;
using WebKit;
using System.IO;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class OnBoardingViewController : AbstractViewController, IWKNavigationDelegate, IUIScrollViewDelegate
    {
        public int VersionCode { get; set; }

        UIView mainView;
        WKWebView webView;
        UIButton okButton;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = new UIColor(0f, 0.0f);

            mainView = new UIView
            {
                LayoutMargins = new UIEdgeInsets(50f, 50f, 50f, 50f),
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            View.Add(mainView);

            View.AddConstraints(new[]
            {
                mainView.LeadingAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.LeadingAnchor : View.LeadingAnchor, 50f),
                mainView.TrailingAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TrailingAnchor : View.TrailingAnchor, -50f),
                mainView.TopAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TopAnchor : View.TopAnchor, 50f),
                mainView.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : View.BottomAnchor, -50f)
            });


            var wkPreferences = new WKPreferences
            {
                JavaScriptEnabled = false
            };

            var config = new WKWebViewConfiguration
            {
                SuppressesIncrementalRendering = false,
                AllowsInlineMediaPlayback = false,
                Preferences = wkPreferences,
                DataDetectorTypes = WKDataDetectorTypes.None,
                WebsiteDataStore = WKWebsiteDataStore.NonPersistentDataStore
            };

            webView = new WKWebView(CGRect.Empty, config)
            {
                Hidden = false,
                NavigationDelegate = this,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            webView.ScrollView.Bounces = false;
            webView.ScrollView.BouncesZoom = false;
            webView.ScrollView.Delegate = this;
            webView.ScrollView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.None;

            mainView.Add(webView);

            mainView.AddConstraints(new[]
            {
                webView.TopAnchor.ConstraintEqualTo(mainView.TopAnchor),
                webView.WidthAnchor.ConstraintEqualTo(mainView.WidthAnchor),
                webView.CenterXAnchor.ConstraintEqualTo(mainView.CenterXAnchor),
                webView.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor)
            });

            okButton = new UIButton
            {
                TintColor = Theme.LightGray,
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            okButton.SetTitleColor(Theme.White, UIControlState.Normal);
            okButton.SetTitle(Localization.GetString("ok"), UIControlState.Normal);

            mainView.Add(okButton);

            mainView.AddConstraints(new[]
            {
                okButton.HeightAnchor.ConstraintEqualTo(65f),
                okButton.WidthAnchor.ConstraintEqualTo(mainView.WidthAnchor),
                okButton.CenterXAnchor.ConstraintEqualTo(mainView.CenterXAnchor),
                okButton.TopAnchor.ConstraintEqualTo(webView.BottomAnchor),
                okButton.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor)
            });
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            UIView.Animate(0.2, () =>
            {
                View.BackgroundColor = new UIColor(0f, 0.5f);
            });
                

            okButton.TouchUpInside += CancelButton_TouchUpInside;

            var html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/changelogs/changelog_" + VersionCode, "html"));
            webView?.StopLoading();
            webView?.LoadHtmlString(html, null);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            okButton.TouchUpInside -= CancelButton_TouchUpInside;
        }

        void CancelButton_TouchUpInside(object sender, EventArgs e)
        {
            UIView.Animate(0.2, () =>
            {
                View.BackgroundColor = new UIColor(0f, 0.0f);
            }, () => 
            {
                DismissViewController(true, null);
            });
        }
    }

    /*public class OnBoardingPresentationController : UIPresentationController, IUIViewControllerTransitioningDelegate
    {
        public OnBoardingPresentationController(UIViewController presentedViewController, UIViewController presentingViewController) : base(presentedViewController, presentingViewController)
        {
        }

        public override CGRect FrameOfPresentedViewInContainerView => ContainerView.Bounds.Inset(100, 100);

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
    }*/
}