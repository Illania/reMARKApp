using System;
using CoreGraphics;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class OnBoardingViewController : AbstractViewController, IWKNavigationDelegate, IUIScrollViewDelegate
    {
        public string ChangelogHtml { get; set; }

        UIView mainView;
        UILabel titleTextView;
        WKWebView webView;
        UIButton okButton;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = new UIColor(0f, 0.0f);

            mainView = new UIView
            {
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            View.Add(mainView);

            View.AddConstraints(new[]
            {
                mainView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                mainView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor)
            });

            if (Integration.IsIPhone())
            {
                View.AddConstraints(new[]
                {
                    mainView.WidthAnchor.ConstraintEqualTo(View.WidthAnchor),
                    mainView.HeightAnchor.ConstraintEqualTo(View.HeightAnchor)
                });
            }
            else
            {
                View.AddConstraints(new[]
                {
                    mainView.WidthAnchor.ConstraintEqualTo(500f),
                    mainView.HeightAnchor.ConstraintEqualTo(750f)
                });
            }

            titleTextView = new UILabel
            {
                UserInteractionEnabled = false,
                Font = UIFont.SystemFontOfSize(40f),
                Text = Localization.GetString("whats_new"),
                TextColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            mainView.Add(titleTextView);

            mainView.AddConstraints(new[]
            {
                titleTextView.TopAnchor.ConstraintEqualTo( Integration.IsRunningAtLeast(11) ?
                                                          mainView.SafeAreaLayoutGuide.TopAnchor : mainView.TopAnchor,
                                                          Integration.IsRunningAtLeast(11) || Integration.IsIPad() ? 0: 20f),
                titleTextView.TrailingAnchor.ConstraintEqualTo(mainView.TrailingAnchor),
                titleTextView.LeadingAnchor.ConstraintEqualTo(mainView.LeadingAnchor, 20f),
            });

            var wkPreferences = new WKPreferences
            {
                JavaScriptEnabled = false
            };

            var config = new WKWebViewConfiguration
            {
                AllowsInlineMediaPlayback = false,
                DataDetectorTypes = WKDataDetectorTypes.None,
                Preferences = wkPreferences,
                SuppressesIncrementalRendering = false,
                WebsiteDataStore = WKWebsiteDataStore.NonPersistentDataStore
            };

            webView = new WKWebView(CGRect.Empty, config)
            {
                Hidden = false,
                NavigationDelegate = this,
                TranslatesAutoresizingMaskIntoConstraints = false,
                UserInteractionEnabled = true
            };

            webView.ScrollView.Bounces = false;
            webView.ScrollView.BouncesZoom = false;
            webView.ScrollView.Delegate = this;
            webView.ScrollView.ScrollEnabled = true;
            webView.ScrollView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.None;
            webView.ScrollView.ShowsVerticalScrollIndicator = true;

            mainView.Add(webView);

            mainView.AddConstraints(new[]
            {
                webView.TopAnchor.ConstraintEqualTo(titleTextView.BottomAnchor),
                webView.TrailingAnchor.ConstraintEqualTo(mainView.TrailingAnchor),
                webView.LeadingAnchor.ConstraintEqualTo(mainView.LeadingAnchor),
                webView.CenterXAnchor.ConstraintEqualTo(mainView.CenterXAnchor)
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
                okButton.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor),
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

            webView?.LoadHtmlString(ChangelogHtml, null);
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
}