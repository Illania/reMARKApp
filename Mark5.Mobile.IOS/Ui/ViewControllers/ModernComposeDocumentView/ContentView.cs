using System;
using System.Threading.Tasks;
using WebKit;
using CoreGraphics;
using Foundation;
using System.IO;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ModernComposeDocumentView
{
    public class ContentView : AbstractComposeDocumentView, IWKNavigationDelegate, IWKScriptMessageHandler
    {
        float HeaderHeight = 100f;

        UIView headerView;

        WKWebView wv;

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
            userContentController.AddScriptMessageHandler(this, "enterpressed");

            var configuration = new WKWebViewConfiguration
            {
                SuppressesIncrementalRendering = true,
                AllowsInlineMediaPlayback = false,
                Preferences = preferences,
                UserContentController = userContentController
            };

            wv = new WKWebView(CGRect.Empty, configuration);
            wv.ScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always;
            wv.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(wv);
            AddConstraints(new[]
            {
                wv.TopAnchor.ConstraintEqualTo(TopAnchor),
                wv.BottomAnchor.ConstraintEqualTo(BottomAnchor),
                wv.LeadingAnchor.ConstraintEqualTo(LeadingAnchor),
                wv.TrailingAnchor.ConstraintEqualTo(TrailingAnchor)
            });

            headerView = new UIView();
            headerView.BackgroundColor = UIColor.Red;
            headerView.TranslatesAutoresizingMaskIntoConstraints = false;
            wv.ScrollView.AddSubview(headerView);

            var c1 = headerView.TopAnchor.ConstraintEqualTo(wv.ScrollView.TopAnchor, -HeaderHeight);
            c1.SetIdentifier("c1");

            var c2 = headerView.HeightAnchor.ConstraintEqualTo(HeaderHeight);
            c2.SetIdentifier("c2");
            wv.AddConstraints(new []
            {
                c1,
                headerView.LeadingAnchor.ConstraintEqualTo(wv.ScrollView.LeadingAnchor),
                headerView.WidthAnchor.ConstraintEqualTo(wv.WidthAnchor),
                c2
            });

            var headerButton = new UIButton();
            headerButton.SetTitle("Click", UIControlState.Normal);
            headerButton.TouchUpInside += (sender, e) => {
                HeaderHeight += 50f;

                foreach (var c in wv.Constraints)
                {
                    if (c.GetIdentifier() == "c1")
                        c.Constant = -HeaderHeight;

                    if (c.GetIdentifier() == "c2")
                        c.Constant = HeaderHeight;
                }

                wv.ScrollView.ContentInset = new UIEdgeInsets(HeaderHeight, 0f, 0f, 0f);

                var co = wv.ScrollView.ContentOffset;
                co.Y -= 50f;
                wv.ScrollView.ContentOffset = co;
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

            wv.ScrollView.ContentInset = new UIEdgeInsets(HeaderHeight, 0f, 0f, 0f);
        }

        internal override Task InitializeView()
        {
            var editor = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/editor", "html"));
            //wv.LoadHtmlString(editor, null);
            //wv.LoadRequest(new NSUrlRequest(new NSUrl("http://google.pl")));
            wv.LoadHtmlString(PreviousDocument.HtmlBody, null);

            return Task.CompletedTask;
        }

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        void DecidePolicy(WKWebView wkWebView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler) => decisionHandler(WKNavigationActionPolicy.Cancel);

        [Export("userContentController:didReceiveScriptMessage:")]
        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            var responseDict = message.Body as NSDictionary;
            if (responseDict == null)
                return;

            var loadedValue = responseDict["loaded"] as NSNumber;
            var resizedValue = responseDict["resized"] as NSNumber;
            var domLoadedValue = responseDict["domloaded"] as NSNumber;
            var mutatedValue = responseDict["mutated"] as NSNumber;
            var enterPressedValue = responseDict["enterpressed"] as NSNumber;

            var loaded = loadedValue != null && loadedValue.BoolValue;
            var resized = resizedValue != null && resizedValue.BoolValue;
            var domLoaded = domLoadedValue != null && domLoadedValue.BoolValue;
            var mutated = mutatedValue != null && mutatedValue.BoolValue;
            var enterPressed = enterPressedValue != null && enterPressedValue.BoolValue;

            if (enterPressed)
            {

            }

            if (loaded || resized || mutated)
            {
            }

            wv.SetNeedsLayout();
            wv.LayoutIfNeeded();
        }
    }
}
