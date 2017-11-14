using System;
using System.Threading.Tasks;
using WebKit;
using CoreGraphics;
using Foundation;
using System.IO;
using UIKit;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ModernComposeDocumentView
{
    public class ContentView : AbstractComposeDocumentView, IWKNavigationDelegate, IWKScriptMessageHandler
    {
        float HeaderHeight = 100f;

        UIView headerView;

        WKWebView webView;

        public ContentView()
        {
            Initialize();
        }

        void Initialize()
        {
            var preferences = new WKPreferences
            {
                MinimumFontSize = 12f,
                JavaScriptCanOpenWindowsAutomatically = false,
                JavaScriptEnabled = true
            };

            var userContentController = new WKUserContentController();
            userContentController.AddScriptMessageHandler(this, "loaded");
            userContentController.AddScriptMessageHandler(this, "resized");
            userContentController.AddScriptMessageHandler(this, "domloaded");
            userContentController.AddScriptMessageHandler(this, "mutated");
            userContentController.AddScriptMessageHandler(this, "keypressed");
            userContentController.AddScriptMessageHandler(this, "enterpressed");

            var configuration = new WKWebViewConfiguration
            {
                SuppressesIncrementalRendering = true,
                AllowsInlineMediaPlayback = false,
                Preferences = preferences,
                UserContentController = userContentController
            };

            webView = new WKWebView(CGRect.Empty, configuration);
            webView.ScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always;
            webView.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(webView);
            AddConstraints(new[]
            {
                webView.TopAnchor.ConstraintEqualTo(TopAnchor),
                webView.BottomAnchor.ConstraintEqualTo(BottomAnchor),
                webView.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                webView.TrailingAnchor.ConstraintEqualTo(TrailingAnchor)
            });

            headerView = new UIView();
            headerView.BackgroundColor = UIColor.Red;
            headerView.TranslatesAutoresizingMaskIntoConstraints = false;
            webView.ScrollView.AddSubview(headerView);

            var c1 = headerView.TopAnchor.ConstraintEqualTo(webView.ScrollView.TopAnchor, -HeaderHeight);
            c1.SetIdentifier("c1");

            var c2 = headerView.HeightAnchor.ConstraintEqualTo(HeaderHeight);
            c2.SetIdentifier("c2");

            webView.AddConstraints(new []
            {
                c1,
                headerView.LeadingAnchor.ConstraintEqualTo(webView.ScrollView.LeadingAnchor),
                headerView.WidthAnchor.ConstraintEqualTo(webView.WidthAnchor),
                c2
            });

            var headerButton = new UIButton();
            headerButton.SetTitle("Click", UIControlState.Normal);
            headerButton.TouchUpInside += (sender, e) => {
                HeaderHeight += 50f;

                foreach (var c in webView.Constraints)
                {
                    if (c.GetIdentifier() == "c1")
                        c.Constant = -HeaderHeight;

                    if (c.GetIdentifier() == "c2")
                        c.Constant = HeaderHeight;
                }

                webView.ScrollView.ContentInset = new UIEdgeInsets(HeaderHeight, 0f, 0f, 0f);

                var co = webView.ScrollView.ContentOffset;
                co.Y -= 50f;
                webView.ScrollView.ContentOffset = co;
            };
            headerButton.TranslatesAutoresizingMaskIntoConstraints = false;
            headerView.AddSubview(headerButton);
            headerView.AddConstraints(new []
            {
                headerButton.TopAnchor.ConstraintEqualTo(headerView.TopAnchor),
                headerButton.BottomAnchor.ConstraintEqualTo(headerView.BottomAnchor),
                headerButton.LeftAnchor.ConstraintEqualTo(headerView.LeftAnchor),
                headerButton.RightAnchor.ConstraintEqualTo(headerView.RightAnchor)
            });

            webView.ScrollView.ContentInset = new UIEdgeInsets(HeaderHeight, 0f, 0f, 0f);
        }

        internal override Task InitializeView()
        {
            var editor = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/editor", "html"));
            webView.LoadHtmlString(editor, null);
            //wv.LoadRequest(new NSUrlRequest(new NSUrl("http://google.pl")));
            //wv.LoadHtmlString(PreviousDocument.HtmlBody, null);

            return Task.CompletedTask;
        }

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        void DecidePolicy(WKWebView wkWebView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler) => decisionHandler(WKNavigationActionPolicy.Cancel);

        [Export("userContentController:didReceiveScriptMessage:")]
        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            var messageName = message?.Name;
            if (messageName == null)
                return;

            CommonConfig.Logger.Debug("JSMsg: " + messageName);

            if (messageName == "loaded")
            {
            }
            if (messageName == "resized")
            {
            }
            if (messageName == "domloaded")
            {
            }
            if (messageName == "mutated")
            {
            }
            if (messageName == "keypressed")
            {
            }
            if (messageName == "enterpressed")
            {
            }
        }
    }
}
