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
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class CustomWebView : WKWebView
    {
        public CustomWebView(CGRect frame, WKWebViewConfiguration configuration, bool allowPasteAsText = false)
            : base(frame, configuration)
        {
            if (allowPasteAsText)
            {
                var pasteMenuItem = new UIMenuItem("Paste as plain text", new Selector("pasteAsText:"));
                UIMenuController.SharedMenuController.MenuItems = new[] { pasteMenuItem };
                UIMenuController.SharedMenuController.Update();
            }
        }

        [Export("pasteAsText:")]
        void PasteAsText(UIMenuController controller)
        {
            var stringToPaste = UIPasteboard.General.String
                .Replace("\r", "")
                .Replace("\n", "\\n");

            var js = $"PastePlain(\'{stringToPaste}\')";
            EvaluateJavaScript("javascript: " + js, completionHandler: (result, error) =>
            {
                if (error != null)
                    CommonConfig.Logger.Debug("Error while pasting as text: " + error);
            });
        }

        public override bool CanPerform(Selector action, NSObject withSender)
        {
            if (action.Name == "pasteAsText:" && UIPasteboard.General.HasStrings)
                return true;

            return base.CanPerform(action, withSender);
        }
    }

    public abstract class AbstractWebViewController : AbstractViewController, IWKNavigationDelegate, IWKScriptMessageHandler, IUIScrollViewDelegate
    {
        public int LoadTimeoutMiliseconds { get; set; } = 2500;

        protected bool IsLoading => webView?.IsLoading ?? false;

        UIActivityIndicatorView loadIndicatorView;
        CustomWebView webView;
        UIView headerContainerView;
        UIView headerView;
        UIProgressView webViewProgressView;

        TaskCompletionSource<bool> loadTcs;

        string headerPaddingJsTemplate;
        bool headerAnimationRunning;

        nfloat keyboardHeight;

        NSObject keyboardDisShowNotification;

        protected bool allowsPasteAsText;

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
            userContentController.AddScriptMessageHandler(this, "input");
            userContentController.AddScriptMessageHandler(this, "onFilePaste");

            var configuration = new WKWebViewConfiguration
            {
                SuppressesIncrementalRendering = false,
                AllowsInlineMediaPlayback = false,
                Preferences = preferences,
                UserContentController = userContentController,
                DataDetectorTypes = WKDataDetectorTypes.All,
                WebsiteDataStore = WKWebsiteDataStore.NonPersistentDataStore,
                SelectionGranularity = WKSelectionGranularity.Character
            };

            webView = new CustomWebView(CGRect.Empty, configuration, allowsPasteAsText)
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
                webViewProgressView.TopAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TopAnchor : View.TopAnchor),
                webViewProgressView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                webViewProgressView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            keyboardDisShowNotification = UIKeyboard.Notifications.ObserveDidShow(HandleKeyboardDidShow);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (webView == null || headerAnimationRunning)
                return;

            var desiredHeaderHeight = headerContainerView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize).Height;
            if (desiredHeaderHeight < 1)
                return;

            SetHeaderPadding(desiredHeaderHeight / webView.ScrollView.ZoomScale);
        }

        protected override void Recycle()
        {
            base.Recycle();

            webView.NavigationDelegate = null;
            webView.ScrollView.Delegate = null;

            keyboardDisShowNotification.Dispose();

            var userContentController = webView?.Configuration?.UserContentController;
            if (userContentController != null)
            {
                userContentController.RemoveScriptMessageHandler("loaded");
                userContentController.RemoveScriptMessageHandler("resized");
                userContentController.RemoveScriptMessageHandler("domloaded");
                userContentController.RemoveScriptMessageHandler("mutated");
                userContentController.RemoveScriptMessageHandler("input");
            }

            webView.RemoveObserver(this, new NSString("estimatedProgress"));
            webView.RemoveObserver(this, new NSString("loading"));

            if (webView.IsLoading)
                webView.StopLoading();

            webViewProgressView?.RemoveFromSuperview();
            loadIndicatorView?.RemoveFromSuperview();
            headerContainerView?.RemoveFromSuperview();
            webView.Hidden = true;
            if (Integration.IsIPhone())
                webView?.RemoveFromSuperview(); // This has been commented out to avoid eventual crashes 
                                                // Github link: https://github.com/xamarin/xamarin-macios/issues/4130#issuecomment-399243880
            webViewProgressView = null;
            loadIndicatorView = null;
            headerContainerView = null;
            headerView = null;
            webView = null;
        }

        void HandleKeyboardDidShow(object sender, UIKeyboardEventArgs e)
        {
            keyboardHeight = e.FrameEnd.Height;
            if (Integration.IsRunningAtLeast(11))
                keyboardHeight -= View.SafeAreaInsets.Bottom;
        }

        protected void HeaderView_BeginAnimating(object sender, EventArgs e) => headerAnimationRunning = true;

        protected void HeaderView_EndAnimating(object sender, EventArgs e) => headerAnimationRunning = false;

        protected void HeaderView_Animating(object sender, EventArgs e) => SetHeaderPadding(headerView.Layer.PresentationLayer.Frame.Height / webView.ScrollView.ZoomScale);

        protected void SetHeaderView(UIView headerView)
        {
            this.headerView = headerView;

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
            if (webView != null)
                webView.Hidden = true;

            loadIndicatorView?.StopAnimating();
        }

        public void ResetOffset()
        {
            webView?.ScrollView?.SetContentOffset(CGPoint.Empty, true);
        }

        protected async Task LoadHtmlString(string html, HtmlProcessingConfiguration config)
        {
            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info("Starting processing of the document...");

            var sw = Stopwatch.StartNew();

            html = await HtmlUtilities.ProcessHtml(html, config);

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

            text = await HtmlUtilities.ProcessPlainText(text, config);

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

        protected void LoadEditorWithPreviousContent(string previousDocumentContent)
        {
            var html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/replyEditor", "html"));

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
            var previousContentNode = bodyNode?.SelectSingleNode("//div[@id='previousContent']");

            if (previousContentNode == null)
                CommonConfig.Logger.Error("resources/html/replyEditor.html is missing 'previousContent' element");
            else
            {
                var previousContentDocument = new HtmlDocument();
                previousContentDocument.LoadHtml(previousDocumentContent);
                var prevBody = previousContentDocument.DocumentNode.SelectSingleNode("//body");

                if (prevBody == null)
                    previousContentNode.InnerHtml = previousDocumentContent;
                else
                    previousContentNode.AppendChildren(prevBody.ChildNodes);

                html = htmlDocument.DocumentNode.OuterHtml;
            }

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

        protected Task<(NSObject, NSError)> InsertTemplate(ContentType contentType, int id, string content)
        {
            var tcs = new TaskCompletionSource<(NSObject, NSError)>();
            string sanitizedContent = string.Empty;
            string typeString = string.Empty;

            if (contentType == ContentType.Html)
            {
                typeString = "html";

                var htmlTemplate = new HtmlDocument();
                htmlTemplate.LoadHtml(content);
                var elementsWithStyleAttribute = htmlTemplate.DocumentNode.SelectNodes("//*[@style]");

                foreach (var element in elementsWithStyleAttribute)
                {
                    var currStyle = element.GetAttributeValue("style", "");
                    element.SetAttributeValue("style", currStyle.Replace("'", string.Empty).Replace("\"", "'"));
                }

                sanitizedContent = htmlTemplate.DocumentNode.OuterHtml.Replace("\"", "'");
            }
            else
            {
                typeString = "text";
                sanitizedContent = content.Replace("\"", "'");
            }

            var js = $"InsertContent(\'{typeString}\',{id},\"{sanitizedContent}\")";
            webView.EvaluateJavaScript("javascript: " + js, (result, error) => tcs.SetResult((result, error)));
            webView.BecomeFirstResponder();
            webView.EndEditing(true);
            return tcs.Task;
        }

        void MoveViewToCaret(int caretPosition)
        {
            var bottomCaretPosition = caretPosition + 30; //Line height

            var verticalOffset = bottomCaretPosition - (webView.ScrollView.Bounds.Height - keyboardHeight - webView.ScrollView.ContentInset.Top); //The last is only for iOS 10 

            CGPoint offset;

            if (verticalOffset > 0)
            {
                offset = new CGPoint(webView.ScrollView.ContentOffset.X, verticalOffset + webView.ScrollView.ContentOffset.Y);
                webView.ScrollView.SetContentOffset(offset, true);
            }
            else if (caretPosition < 0)
            {
                var yOffset = webView.ScrollView.ContentOffset.Y + caretPosition;
                offset = new CGPoint(webView.ScrollView.ContentOffset.X, yOffset < 0 ? 0 : yOffset);
                webView.ScrollView.SetContentOffset(offset, true);
            }
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (webView == null || webViewProgressView == null)
                return;

            if (ofObject == webView && keyPath == "estimatedProgress")
                webViewProgressView?.SetProgress((float)webView.EstimatedProgress, true);

            if (ofObject == webView && keyPath == "loading")
                UIView.AnimateNotify(.2d, () =>
                {
                    webViewProgressView.Alpha = webView.IsLoading ? 1f : 0f;
                }, (finished) =>
                {
                    if (finished && webView?.IsLoading == false)
                        webViewProgressView?.SetProgress(0f, false);
                });
        }

        [Export("scrollViewDidScroll:")]
        public void Scrolled(UIScrollView scrollView)
        {
            if (webView == null)
                return;

            var constraint = webView.Constraints.FirstOrDefault(c => c.GetIdentifier() == "headerContainer.leadingAnchor");
            if (constraint == null)
                return;
            constraint.Constant = scrollView.ContentOffset.X;
        }

        [Export("scrollViewDidZoom:")]
        public void DidZoom(UIScrollView scrollView)
        {
            if (webView == null)
                return;

            if (headerContainerView.Bounds.Height > 0)
                SetHeaderPadding(headerContainerView.Bounds.Height / webView.ScrollView.ZoomScale);
        }

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            decisionHandler(CanNavigate(navigationAction) ? WKNavigationActionPolicy.Allow : WKNavigationActionPolicy.Cancel);
        }

        [Export("webView:didFinishNavigation:")]
        void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (headerContainerView.Bounds.Height > 0)
                SetHeaderPadding(headerContainerView.Bounds.Height / webView.ScrollView.ZoomScale);

            loadTcs.SetResult(true);
        }

        [Export("webView:didFailNavigation:withError:")]
        void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error) => loadTcs.SetResult(false);

        void SetHeaderPadding(nfloat height)
        {
            var headerPaddingJs = headerPaddingJsTemplate;
            headerPaddingJs = HtmlUtilities.ProcessWebTemplate(headerPaddingJs, (int)height);
            webView?.EvaluateJavaScript(headerPaddingJs, null);
        }

        void IWKScriptMessageHandler.DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            var messageName = message?.Name;
            if (messageName == null)
                return;

            var messageBody = message?.Body?.ToString();

            if (messageName == "domloaded")
                OnWebViewDomLoaded();

            if (messageName == "loaded")
                OnWebViewLoaded();

            if (messageName == "resized")
                OnWebViewResized();

            if (messageName == "mutated")
                OnWebViewMutated();

            if (messageName == "input" && int.TryParse(messageBody, out int caretYposition))
                OnWebViewInput(caretYposition);

            if (messageName == "onFilePaste")
                OnFilePaste();
        }

        protected virtual void OnWebViewDomLoaded() { }

        protected virtual void OnWebViewLoaded() { }

        protected virtual void OnWebViewResized() { }

        protected virtual void OnWebViewMutated() { }

        protected virtual void OnWebViewInput(int caretPosition)
        {
            MoveViewToCaret(caretPosition);
        }

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

        protected async void OnFilePaste()
        {
            await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("unable_to_paste_title"), Localization.GetString("unable_to_paste_content"));
        }
    }
}