using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CoreGraphics;
using Foundation;
using HtmlAgilityPack;
using MailBee.Html;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using WebKit;

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

        string headerPaddingJsTemplate;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            headerPaddingJsTemplate = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/headerPadding", "js"));

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
                DataDetectorTypes = WKDataDetectorTypes.All,
                WebsiteDataStore = WKWebsiteDataStore.NonPersistentDataStore
            };

            webView = new WKWebView(CGRect.Empty, configuration)
            {
                Hidden = true,
                NavigationDelegate = this,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            webView.ScrollView.Bounces = false;
            webView.ScrollView.BouncesZoom = false;
            webView.ScrollView.Delegate = this;
            webView.ScrollView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            webView.AddObserver(this, new NSString("estimatedProgress"), NSKeyValueObservingOptions.New, IntPtr.Zero);
            webView.AddObserver(this, new NSString("loading"), NSKeyValueObservingOptions.New, IntPtr.Zero);
            View.AddSubview(webView);

            if (Integration.IsRunningAtLeast(11))
            {
                View.AddConstraints(new[]
                {
                    webView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                    webView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                    webView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                    webView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor)
                });
            }
            else
            {
                View.AddConstraints(new[]
                {
                    webView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                    webView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                    webView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    webView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
                });
            }

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

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            var desireHeaderSize = headerContainerView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize);
            var desiredHeaderHeight = desireHeaderSize.Height;
            if (desiredHeaderHeight < 1)
                return;

            var constraint = webView.Constraints.FirstOrDefault(c => c.GetIdentifier() == "headerContainer.height");
            if (constraint == null)
                return;
            constraint.Constant = desiredHeaderHeight;

            var headerPaddingJs = headerPaddingJsTemplate;
            headerPaddingJs = ProcessWebTemplate(headerPaddingJs, desireHeaderSize.Height);
            webView?.EvaluateJavaScript(headerPaddingJs, null);
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

                CommonConfig.Logger.Warning("!!! Email load timeout !!!");
            }

            webView.Hidden = false;
            loadIndicatorView.StopAnimating();
        }

        protected void Clear()
        {
            webView.Hidden = true;
            loadIndicatorView.StopAnimating();
        }

        protected async Task LoadHtmlString(string html, HtmlProcessingConfiguration config)
        {
            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info("Starting processing of the document...");

            var sw = Stopwatch.StartNew();

            html = await ProcessHtml(html, config);

            sw.Stop();

            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info($"Processing document took: {sw.ElapsedMilliseconds}ms.");

            webView?.StopLoading();
            webView?.LoadHtmlString(html, NSBundle.MainBundle.BundleUrl);
        }

        protected async Task LoadPlainText(string text, PlainTextProcessingConfiguration config)
        {
            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info("Starting processing of the document...");

            var sw = Stopwatch.StartNew();

            text = await ProcessPlainText(text, config);

            sw.Stop();

            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info($"Processing document took: {sw.ElapsedMilliseconds}ms.");

            webView?.StopLoading();
            webView?.LoadHtmlString(text, null);
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

        protected void LoadEditor()
        {
            var html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/editor", "html"));
            webView?.StopLoading();
            webView?.LoadHtmlString(html, null);
        }

        protected virtual async Task<string> GetContent()
        {
            return await webView?.EvaluateJavaScriptAsync("document.documentElement.outerHTML") as NSString;
        }

        protected Task<(NSObject, NSError)> EvaluateJavaScriptAsync(string script)
        {
            var tcs = new TaskCompletionSource<(NSObject, NSError)>();
            webView.EvaluateJavaScript(script, (result, error) => tcs.SetResult((result, error)));
            return tcs.Task;
        }

        protected async Task<string> ProcessHtml(string html, HtmlProcessingConfiguration config)
        {
            var sw = Stopwatch.StartNew();

            if (config.MakeHtmlSafe)
            {
                html = await MakeHtmlSafe(html);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"MakeHtmlSafe {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.MakeHtmlKindaSafe)
            {
                html = await MakeHtmlKindaSafe(html);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"MakeHtmlKindaSafe {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.InlineCss)
            {
                html = await InlineCss(html);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"InlineCss {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug($"LoadHtml {sw.ElapsedMilliseconds}ms");
            sw.Restart();

            if (config.CorrectScale)
            {
                await CorrectScale(htmlDocument);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"CorrectScale {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.InjectFonts)
            {
                await InjectFonts(htmlDocument);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"InjectFonts {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.MakeEditable)
            {
                await MakeEditable(htmlDocument);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"MakeEditable {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            if (config.InjectReplyHeader)
            {
                await InjectReplyHeader(htmlDocument, config.ReplyHeaderParameters);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"InjectReplyHeader {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }

            sw.Stop();

            return htmlDocument.DocumentNode.OuterHtml;
        }

        protected async Task<string> ProcessPlainText(string text, PlainTextProcessingConfiguration config)
        {
            if (config.Encode)
                text = HttpUtility.HtmlEncode(text);

            var html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/plain", "html"));
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var preNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='plaintext']");
            preNode.InnerHtml = text;

            if (config.MakeEditable)
                await MakeEditable(htmlDocument);

            if (config.InjectReplyHeader)
                await InjectReplyHeader(htmlDocument, config.ReplyHeaderParameters);

            return htmlDocument.DocumentNode.OuterHtml;
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

        Task<string> MakeHtmlKindaSafe(string html)
        {
            return Task.Run(() =>
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                var dn = htmlDocument.DocumentNode;

                var nodesToRemove = new List<HtmlNode>();

                foreach (var xpath in new[] { "//script", "//bgsound", "//embed", "//iframe", "//frame", "//frameset", "//object", "//applet" })
                {
                    var nodes = dn.SelectNodes(xpath);
                    if (nodes != null)
                        nodesToRemove.AddRange(nodes);
                }

                foreach (var nodeToRemove in nodesToRemove)
                    nodeToRemove.Remove();

                return htmlDocument.DocumentNode.OuterHtml;
            });
        }

        Task<string> InlineCss(string html)
        {
            return Task.Run(() =>
            {
                if (html == null)
                    return null;

                var inlineResult = PreMailer.Net.PreMailer.MoveCssInline(html, true, null, null, true, true);
                return inlineResult.Html;
            });
        }

        Task CorrectScale(HtmlDocument htmlDocument)
        {
            return Task.Run(() =>
            {
                var headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");
                if (headNode == null)
                    return;

                var existingViewportNodes = headNode.SelectNodes("/meta[@name='viewport']");
                if (existingViewportNodes != null)
                    foreach (var existingViewportNode in existingViewportNodes)
                        existingViewportNode.Remove();

                var viewportElement = htmlDocument.CreateElement("meta");
                viewportElement.SetAttributeValue("id", "viewport");
                viewportElement.SetAttributeValue("name", "viewport");
                viewportElement.SetAttributeValue("content", "initial-scale=1, minimum-scale=0.75, maximum-scale=1.25, user-scalable=yes");
                headNode.PrependChild(viewportElement);
            });
        }

        Task InjectFonts(HtmlDocument htmlDocument)
        {
            return Task.Run(() =>
            {
                var headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");
                if (headNode == null)
                    return;

                var cssLinkElement = htmlDocument.CreateElement("link");
                cssLinkElement.SetAttributeValue("id", "fonts");
                cssLinkElement.SetAttributeValue("rel", "stylesheet");
                cssLinkElement.SetAttributeValue("type", "text/css");
                cssLinkElement.SetAttributeValue("href", "html/fonts.css");
                headNode.PrependChild(cssLinkElement);
            });
        }

        Task MakeEditable(HtmlDocument htmlDocument)
        {
            return Task.Run(() =>
            {
                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
                if (bodyNode == null)
                    return;

                bodyNode.SetAttributeValue("contentEditable", "true");
            });
        }

        Task InjectReplyHeader(HtmlDocument htmlDocument, string[] parameters)
        {
            return Task.Run(() =>
            {
                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
                if (bodyNode == null)
                    return;

                var replyHeader = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/replyHeader", "html"));
                replyHeader = ProcessWebTemplate(replyHeader, parameters);
                var headerDiv = htmlDocument.CreateElement("div");
                headerDiv.SetAttributeValue("id", "replyHeader");
                headerDiv.InnerHtml = replyHeader;
                bodyNode.PrependChild(headerDiv);
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
            var constraint = webView.Constraints.FirstOrDefault(c => c.GetIdentifier() == "headerContainer.leadingAnchor");
            if (constraint == null)
                return;
            constraint.Constant = scrollView.ContentOffset.X;
        }

        [Export("scrollViewDidZoom:")]
        public void DidZoom(UIScrollView scrollView)
        {
            if (headerContainerView.Bounds.Height > 0)
            {
                var headerPaddingJs = headerPaddingJsTemplate;
                headerPaddingJs = ProcessWebTemplate(headerPaddingJs, headerContainerView.Bounds.Height / webView.ScrollView.ZoomScale);
                webView?.EvaluateJavaScript(headerPaddingJs, null);
            }
        }

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            decisionHandler(CanNavigate(navigationAction) ? WKNavigationActionPolicy.Allow : WKNavigationActionPolicy.Cancel);
        }

        [Export("webView:didFinishNavigation:")]
        async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (headerContainerView.Bounds.Height > 0)
            {
                var headerPaddingJs = headerPaddingJsTemplate;
                headerPaddingJs = ProcessWebTemplate(headerPaddingJs, headerContainerView.Bounds.Height / webView.ScrollView.ZoomScale);
                await webView?.EvaluateJavaScriptAsync(headerPaddingJs);
            }

            loadTcs.SetResult(true);
        }

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

        protected class HtmlProcessingConfiguration
        {
            public static HtmlProcessingConfiguration DefaultForViewing
            {
                get => new HtmlProcessingConfiguration();
            }

            public static HtmlProcessingConfiguration DefaultForEditing
            {
                get => new HtmlProcessingConfiguration
                {
                    InlineCss = true,
                    MakeEditable = true
                };
            }

            public static HtmlProcessingConfiguration Disabled
            {
                get => new HtmlProcessingConfiguration
                {
                    MakeHtmlKindaSafe = false,
                    CorrectScale = false,
                    InjectFonts = false
                };
            }

            public bool MakeHtmlSafe { get; set; } = false;
            public bool MakeHtmlKindaSafe { get; set; } = true;
            public bool CorrectScale { get; set; } = true;
            public bool InjectFonts { get; set; } = true;
            public bool InlineCss { get; set; } = false;
            public bool MakeEditable { get; set; } = false;
            public bool InjectReplyHeader { get; set; } = false;

            public string[] ReplyHeaderParameters { get; set; }
        }

        protected class PlainTextProcessingConfiguration
        {
            public static PlainTextProcessingConfiguration DefaultForViewing
            {
                get => new PlainTextProcessingConfiguration();
            }

            public static PlainTextProcessingConfiguration DefaultForEditing
            {
                get => new PlainTextProcessingConfiguration
                {
                    MakeEditable = true
                };
            }

            public static PlainTextProcessingConfiguration Disabled
            {
                get => new PlainTextProcessingConfiguration
                {
                    Encode = false,
                };
            }

            public bool Encode { get; set; } = true;
            public bool MakeEditable { get; set; } = false;
            public bool InjectReplyHeader { get; set; } = false;

            public string[] ReplyHeaderParameters { get; set; }
        }

        protected static string ProcessWebTemplate(string template, params object[] args)
        {
            var output = template;
            for (var i = 0; i < args.Length; i++)
                output = output.Replace($"%%{i}%%", args[i].ToString());
            return output;
        }
    }
}