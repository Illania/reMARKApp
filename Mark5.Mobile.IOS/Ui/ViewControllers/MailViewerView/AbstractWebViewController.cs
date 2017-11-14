using System;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using MailBee.Html;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using WebKit;
using System.Linq;
using HtmlAgilityPack;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView
{
    public abstract class AbstractWebViewController : AbstractViewController, IWKNavigationDelegate, IWKScriptMessageHandler
    {
        UIView headerContainer;
        WKWebView webView;
        UIProgressView progressView;

        public override void LoadView()
        {
            base.LoadView();

            var preferences = new WKPreferences
            {
                MinimumFontSize = 12f,
                JavaScriptCanOpenWindowsAutomatically = false,
                JavaScriptEnabled = true,
            };

            var userContentController = new WKUserContentController();

            var scriptsAtStart = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/scriptsAtStart", "js"));
            var scriptsAtEnd = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/scriptsAtEnd", "js"));

            userContentController.AddUserScript(new WKUserScript(new NSString(scriptsAtStart), WKUserScriptInjectionTime.AtDocumentStart, true));
            userContentController.AddUserScript(new WKUserScript(new NSString(scriptsAtEnd), WKUserScriptInjectionTime.AtDocumentEnd, true));
            userContentController.AddScriptMessageHandler(this, "loaded");
            userContentController.AddScriptMessageHandler(this, "resized");
            userContentController.AddScriptMessageHandler(this, "domloaded");
            userContentController.AddScriptMessageHandler(this, "mutated");
            userContentController.AddScriptMessageHandler(this, "keypressed");
            userContentController.AddScriptMessageHandler(this, "enterpressed");

            var configuration = new WKWebViewConfiguration
            {
                SuppressesIncrementalRendering = false,
                AllowsInlineMediaPlayback = false,
                Preferences = preferences,
                UserContentController = userContentController
            };

            webView = new WKWebView(CGRect.Empty, configuration)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                NavigationDelegate = this
            };
            webView.AddObserver(this, new NSString("estimatedProgress"), NSKeyValueObservingOptions.New, IntPtr.Zero);
            webView.AddObserver(this, new NSString("loading"), NSKeyValueObservingOptions.New, IntPtr.Zero);
            View.AddSubview(webView);
            View.AddConstraints(new[]
            {
                webView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                webView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                webView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                webView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            progressView = new UIProgressView(UIProgressViewStyle.Bar)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(progressView);
            View.AddConstraints(new[]
            {
                progressView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                progressView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                progressView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            headerContainer = new UIView
            {
                Opaque = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            webView.ScrollView.AddSubview(headerContainer);

            var c1 = headerContainer.TopAnchor.ConstraintEqualTo(webView.ScrollView.TopAnchor);
            c1.SetIdentifier("headerContainer.topAnchor");
            var c2 = headerContainer.LeadingAnchor.ConstraintEqualTo(webView.ScrollView.LeadingAnchor);
            c2.SetIdentifier("headerContainer.leadingAnchor");
            var c3 = headerContainer.WidthAnchor.ConstraintEqualTo(webView.WidthAnchor);
            c3.SetIdentifier("headerContainer.widthAnchor");
            var c4 = headerContainer.HeightAnchor.ConstraintGreaterThanOrEqualTo(0);
            c4.SetIdentifier("headerContainer.height");

            webView.AddConstraints(new[] { c1, c2, c3, c4 });
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            var desireHeaderSize = headerContainer.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize);
            var desiredHeaderHeight = desireHeaderSize.Height;

            if (desiredHeaderHeight < 1)
                return;

            foreach (var constraint in webView.Constraints)
            {
                if (constraint.GetIdentifier() == "headerContainer.topAnchor")
                    constraint.Constant = -desiredHeaderHeight;

                if (constraint.GetIdentifier() == "headerContainer.height")
                    constraint.Constant = desiredHeaderHeight;
            }

            var contentInset = webView.ScrollView.ContentInset;
            contentInset.Top = desiredHeaderHeight;
            webView.ScrollView.ContentInset = contentInset;
        }

        protected void SetHeaderView(UIView headerView)
        {
            foreach (var subview in headerContainer.Subviews)
                subview.RemoveFromSuperview();

            headerView.TranslatesAutoresizingMaskIntoConstraints = false;
            headerContainer.AddSubview(headerView);
            headerContainer.AddConstraints(new[]
            {
                headerView.TopAnchor.ConstraintEqualTo(headerContainer.TopAnchor),
                headerView.BottomAnchor.ConstraintEqualTo(headerContainer.BottomAnchor),
                headerView.LeadingAnchor.ConstraintEqualTo(headerContainer.LeadingAnchor),
                headerView.TrailingAnchor.ConstraintEqualTo(headerContainer.TrailingAnchor)
            });
        }

        protected bool IsLoading => webView?.IsLoading ?? false;

        protected async Task LoadHtmlString(string html, bool makeHtmlSafe, bool correctScale, bool inlineCss)
        {
            if (makeHtmlSafe)
                html = await MakeHtmlSafe(html);

            if (correctScale)
                html = await CorrectScale(html);

            if (inlineCss)
                html = await InlineCss(html);

            webView?.StopLoading();
            webView?.LoadHtmlString(html, null);
        }

        protected void LoadPlainString(string plain)
        {
            webView?.StopLoading();
            webView?.LoadData(NSData.FromString(plain), "text/plain", "UTF-8", new NSUrl("/"));
        }

        protected void LoadNoContentString()
        {
            var html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/empty", "html"));
            webView?.StopLoading();
            webView?.LoadHtmlString(html, null);
        }

        protected void StopLoading() => webView?.StopLoading();

        Task<string> MakeHtmlSafe(string html)
        {
            return Task.Run(() =>
            {
                if (html == null)
                    return null;

                var p = new Processor();
                p.Dom.OuterHtml = html;
                var safeHtml = p.Dom.ProcessToString(RuleSet.GetSafeHtmlRules(), null);
                return safeHtml;
            });
        }

        Task<String> CorrectScale(string html)
        {
            return Task.Run(() =>
            {
                if (html == null)
                    return null;

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");
                if (headNode == null)
                    return html;

                var existingViewportNodes = headNode.SelectNodes("/meta[@name=viewport]");
                if (existingViewportNodes != null)
                    foreach (var existingViewportNode in existingViewportNodes)
                        existingViewportNode.Remove();

                var viewportElement = htmlDocument.CreateElement("meta");
                viewportElement.SetAttributeValue("name", "viewport");
                viewportElement.SetAttributeValue("content", "initial-scale=0.8, minimum-scale=0.5, maximum-scale=3, user-scalable=yes");
                headNode.PrependChild(viewportElement);

                var scaledHtml = htmlDocument.DocumentNode.OuterHtml;
                return scaledHtml;
            });
        }

        Task<String> InlineCss(string html)
        {
            return Task.Run(() =>
            {
                if (html == null)
                    return null;

                var inlineResult = PreMailer.Net.PreMailer.MoveCssInline(html, true, null, null, true, true);
                return inlineResult.Html;
            });
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (ofObject == webView && keyPath == "estimatedProgress")
            {
                progressView.SetProgress((float)webView.EstimatedProgress, true);
            }
            if (ofObject == webView && keyPath == "loading")
            {
                UIView.AnimateNotify(.2d, () =>
                {
                    progressView.Alpha = webView.IsLoading ? 1f : 0f;
                }, (finished) =>
                {
                    if (finished && !webView.IsLoading)
                        progressView.SetProgress(0f, false);
                });
            }
        }

        void DecidePolicy(WKWebView wkWebView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            decisionHandler(CanNavigate(navigationAction) ? WKNavigationActionPolicy.Allow : WKNavigationActionPolicy.Cancel);
        }

        void IWKScriptMessageHandler.DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            var messageName = message?.Name;
            if (messageName == null)
                return;

            if (messageName == "domloaded")
                OnWebViewDomLoaded();

            if (messageName == "loaded")
                OnWebViewLoaded();

            if (messageName == "resized")
                OnWebViewResized();

            if (messageName == "mutated")
                OnWebViewMutated();

            if (messageName == "keypressed")
                OnWebViewKeyPressed();

            if (messageName == "enterpressed")
                OnWebViewEnterPressed();
        }

        protected virtual void OnWebViewDomLoaded() { }

        protected virtual void OnWebViewLoaded() { }

        protected virtual void OnWebViewResized() { }

        protected virtual void OnWebViewMutated() { }

        protected virtual void OnWebViewKeyPressed() { }

        protected virtual void OnWebViewEnterPressed() { }

        protected virtual bool CanNavigate(WKNavigationAction action) => false;
    }
}
