using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using CoreGraphics;
using Foundation;
using HtmlAgilityPack;
using MailBee.Html;
using UIKit;
using WebKit;
using Mark5.Mobile.Common;
using System.Diagnostics;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public abstract class AbstractWebViewController : AbstractViewController, IWKNavigationDelegate, IWKScriptMessageHandler, IUIScrollViewDelegate
    {
        public int LoadTimeoutMiliseconds { get; set; } = 2500;

        protected bool IsLoading => webView?.IsLoading ?? false;

        UIActivityIndicatorView loadIndicatorView;
        WKWebView webView;
        UIView headerContainerView;
        UIProgressView webViewProgressView;

        TaskCompletionSource<bool> loadTcs;

        public override void LoadView()
        {
            base.LoadView();

            View.BackgroundColor = Theme.White;

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
                UserContentController = userContentController,
                DataDetectorTypes = WKDataDetectorTypes.All
            };

            webView = new WKWebView(CGRect.Empty, configuration)
            {
                Hidden = true,
                NavigationDelegate = this,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            webView.ScrollView.Delegate = this;
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

            headerContainerView = new UIView
            {
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            webView.ScrollView.AddSubview(headerContainerView);

            var c1 = headerContainerView.TopAnchor.ConstraintEqualTo(webView.ScrollView.TopAnchor);
            c1.SetIdentifier("headerContainer.topAnchor");
            var c2 = headerContainerView.LeadingAnchor.ConstraintEqualTo(webView.ScrollView.LeadingAnchor);
            c2.SetIdentifier("headerContainer.leadingAnchor");
            var c3 = headerContainerView.WidthAnchor.ConstraintEqualTo(webView.WidthAnchor);
            c3.SetIdentifier("headerContainer.widthAnchor");
            var c4 = headerContainerView.HeightAnchor.ConstraintGreaterThanOrEqualTo(0);
            c4.SetIdentifier("headerContainer.height");

            webView.AddConstraints(new[] { c1, c2, c3, c4 });

            loadIndicatorView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray)
            {
                HidesWhenStopped = true,
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(loadIndicatorView);
            View.AddConstraints(new[]
            {
                loadIndicatorView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                loadIndicatorView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                loadIndicatorView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                loadIndicatorView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            webViewProgressView = new UIProgressView(UIProgressViewStyle.Bar)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(webViewProgressView);
            View.AddConstraints(new[]
            {
                webViewProgressView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                webViewProgressView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                webViewProgressView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            var desireHeaderSize = headerContainerView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize);
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

        protected override void Recycle()
        {
            base.Recycle();

            webView.NavigationDelegate = null;
            webView.ScrollView.Delegate = null;

            var userContentController = webView?.Configuration?.UserContentController;
            if (userContentController != null)
            {
                userContentController.RemoveScriptMessageHandler("loaded");
                userContentController.RemoveScriptMessageHandler("resized");
                userContentController.RemoveScriptMessageHandler("domloaded");
                userContentController.RemoveScriptMessageHandler("mutated");
                userContentController.RemoveScriptMessageHandler("keypressed");
                userContentController.RemoveScriptMessageHandler("enterpressed");
            }

            webView.RemoveObserver(this, new NSString("estimatedProgress"));
            webView.RemoveObserver(this, new NSString("loading"));

            if (webView.IsLoading)
                webView.StopLoading();

            webViewProgressView?.RemoveFromSuperview();
            loadIndicatorView?.RemoveFromSuperview();
            headerContainerView?.RemoveFromSuperview();
            webView?.RemoveFromSuperview();

            webViewProgressView = null;
            loadIndicatorView = null;
            headerContainerView = null;
            webView = null;
        }

        protected void SetHeaderView(UIView headerView)
        {
            foreach (var subview in headerContainerView.Subviews)
                subview.RemoveFromSuperview();

            headerView.TranslatesAutoresizingMaskIntoConstraints = false;
            headerContainerView.AddSubview(headerView);
            headerContainerView.AddConstraints(new[]
            {
                headerView.TopAnchor.ConstraintEqualTo(headerContainerView.TopAnchor),
                headerView.BottomAnchor.ConstraintEqualTo(headerContainerView.BottomAnchor),
                headerView.LeadingAnchor.ConstraintEqualTo(headerContainerView.LeadingAnchor),
                headerView.TrailingAnchor.ConstraintEqualTo(headerContainerView.TrailingAnchor)
            });
        }

        protected async Task StartRefreshing()
        {
            webView.Hidden = true;
            loadIndicatorView.StartAnimating();

            if (loadTcs != null)
                await loadTcs.Task;

            loadTcs = new TaskCompletionSource<bool>();
        }

        protected async Task EndRefreshing()
        {
            var loadTask = loadTcs.Task;
            var timeoutTask = Task.Delay(LoadTimeoutMiliseconds);

            var finishedTask = await Task.WhenAny(loadTask, timeoutTask);

            if (finishedTask == timeoutTask)
            {
                if (webView.IsLoading)
                    webView.StopLoading();
                await loadTcs.Task;
            }

            webView.Hidden = false;
            loadIndicatorView.StopAnimating();
        }

        protected void Clear()
        {
            webView.Hidden = true;
            loadIndicatorView.StopAnimating();
        }

        protected async Task LoadHtmlString(string html, bool makeHtmlSafe, bool correctScale, bool inlineCss, bool injectFonts)
        {
            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info("Starting processing of the document...");

            var processingStopwatch = Stopwatch.StartNew();

            if (makeHtmlSafe)
                html = await MakeHtmlSafe(html);

            if (inlineCss)
                html = await InlineCss(html);

            if (correctScale)
                html = await CorrectScale(html);

            if (injectFonts)
                html = await InjectFonts(html);

            processingStopwatch.Stop();

            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info($"Processing document took: {processingStopwatch.ElapsedMilliseconds}ms.");

            webView?.StopLoading();
            webView?.LoadHtmlString(html, NSBundle.MainBundle.BundleUrl);
        }

        protected void LoadPlainString(string plain)
        {
            var escapedPlain = HttpUtility.HtmlEncode(plain);
            var html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/plain", "html"));
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var preNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='plaintext']");
            preNode.InnerHtml = escapedPlain;
            var plainHtml = htmlDocument.DocumentNode.OuterHtml;

            webView?.StopLoading();
            webView?.LoadHtmlString(plainHtml, null);
        }

        protected void LoadNoContentString()
        {
            var html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/empty", "html"));
            webView?.StopLoading();
            webView?.LoadHtmlString(html, null);
        }

        protected void LoadEmpty()
        {
            webView?.StopLoading();
            webView?.LoadHtmlString("", null);
        }

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

                var existingViewportNodes = headNode.SelectNodes("/meta[@name='viewport']");
                if (existingViewportNodes != null)
                    foreach (var existingViewportNode in existingViewportNodes)
                        existingViewportNode.Remove();

                var viewportElement = htmlDocument.CreateElement("meta");
                viewportElement.SetAttributeValue("name", "viewport");
                viewportElement.SetAttributeValue("content", "initial-scale=1");
                headNode.PrependChild(viewportElement);

                var scaledHtml = htmlDocument.DocumentNode.OuterHtml;
                return scaledHtml;
            });
        }

        Task<String> InjectFonts(string html)
        {
            return Task.Run(() =>
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");
                if (headNode == null)
                    return html;

                var cssLinkElement = htmlDocument.CreateElement("link");
                cssLinkElement.SetAttributeValue("rel", "stylesheet");
                cssLinkElement.SetAttributeValue("type", "text/css");
                cssLinkElement.SetAttributeValue("href", "html/fonts.css");
                headNode.PrependChild(cssLinkElement);

                var htmlWithFonts = htmlDocument.DocumentNode.OuterHtml;
                return htmlWithFonts;
            });
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (ofObject == webView && keyPath == "estimatedProgress")
                webViewProgressView.SetProgress((float)webView.EstimatedProgress, true);

            if (ofObject == webView && keyPath == "loading")
                UIView.AnimateNotify(.2d, () =>
                {
                    webViewProgressView.Alpha = webView.IsLoading ? 1f : 0f;
                }, (finished) =>
                {
                    if (finished && !webView.IsLoading)
                        webViewProgressView.SetProgress(0f, false);
                });
        }

        [Export("scrollViewDidScroll:")]
        public void Scrolled(UIScrollView scrollView)
        {
            NSLayoutConstraint leadingAnchorConstraint = null;
            foreach (var constraint in webView.Constraints)
            {
                if (constraint.GetIdentifier() == "headerContainer.leadingAnchor")
                {
                    leadingAnchorConstraint = constraint;
                    break;
                }
            }

            if (leadingAnchorConstraint == null)
                return;

            leadingAnchorConstraint.Constant = scrollView.ContentOffset.X;
        }

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            decisionHandler(CanNavigate(navigationAction) ? WKNavigationActionPolicy.Allow : WKNavigationActionPolicy.Cancel);
        }

        [Export("webView:didFinishNavigation:")]
        void DidFinishNavigation(WKWebView webView, WKNavigation navigation) => loadTcs.SetResult(true);

        [Export("webView:didFailNavigation:withError:")]
        void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error) => loadTcs.SetResult(false);

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

        protected virtual bool CanNavigate(WKNavigationAction action)
        {
            switch (action.NavigationType)
            {
                case WKNavigationType.LinkActivated:
                case WKNavigationType.BackForward:
                case WKNavigationType.FormSubmitted:
                case WKNavigationType.FormResubmitted:
                case WKNavigationType.Reload:
                    return false;
                default:
                    return true;
            }
        }
    }
}
