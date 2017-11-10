using System;
using System.Threading;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using HtmlAgilityPack;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class ContentView : DocumentSubView, IWKNavigationDelegate, IWKScriptMessageHandler
    {
        static readonly NSString script1 = new NSString("window.onload = function () {window.webkit.messageHandlers.sizeNotification.postMessage({justLoaded:true});};");
        static readonly NSString script2 = new NSString("window.onresize = function () {window.webkit.messageHandlers.sizeNotification.postMessage({resized:true});};");
        static readonly NSString script3 = new NSString("document.addEventListener(\"DOMContentLoaded\", function () {window.webkit.messageHandlers.sizeNotification.postMessage({domLoaded:true});});");

        readonly WeakReference<DocumentViewController> viewControllerWeakReference;

        UIActivityIndicatorView spinner;
        WKUserContentController userContentController;
        WKWebViewConfiguration configuration;
        WKWebView webView;
        NSLayoutConstraint webViewHeightConstraint;

        CancellationTokenSource cts;

        public ContentView(DocumentViewController viewController)
        {
            viewControllerWeakReference = viewController.Wrap();
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                spinner?.RemoveFromSuperview();
                spinner = null;

                webView?.RemoveFromSuperview();
                if (webView != null)
                    webView.NavigationDelegate = null;
                webView = null;
                userContentController?.RemoveScriptMessageHandler("sizeNotification");
                userContentController = null;
                configuration = null;

                webViewHeightConstraint = null;
            }
        }

        void CreateWebView()
        {
            spinner?.RemoveFromSuperview();
            spinner = null;

            spinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HidesWhenStopped = true
            };
            spinner.StartAnimating();
            ContainerView.AddSubview(spinner);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(spinner, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(spinner, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
            });

            webView?.RemoveFromSuperview();
            if (webView != null)
                webView.NavigationDelegate = null;
            webView = null;
            userContentController?.RemoveScriptMessageHandler("sizeNotification");
            userContentController = null;
            configuration = null;

            webViewHeightConstraint = null;

            var preferences = new WKPreferences
            {
                MinimumFontSize = 12f,
                JavaScriptCanOpenWindowsAutomatically = false,
                JavaScriptEnabled = true
            };
            var wkscript1 = new WKUserScript(script1, WKUserScriptInjectionTime.AtDocumentEnd, true);
            var wkscript2 = new WKUserScript(script2, WKUserScriptInjectionTime.AtDocumentEnd, true);
            var wkscript3 = new WKUserScript(script3, WKUserScriptInjectionTime.AtDocumentStart, true);

            userContentController = new WKUserContentController();
            userContentController.AddUserScript(wkscript1);
            userContentController.AddUserScript(wkscript2);
            userContentController.AddUserScript(wkscript3);
            userContentController.AddScriptMessageHandler(this, "sizeNotification");

            configuration = new WKWebViewConfiguration
            {
                SuppressesIncrementalRendering = true,
                AllowsInlineMediaPlayback = false,
                UserContentController = userContentController,
                Preferences = preferences,
                DataDetectorTypes = WKDataDetectorTypes.PhoneNumber | WKDataDetectorTypes.Link
            };
            webView = new WKWebView(CGRect.Empty, configuration)
            {
                NavigationDelegate = this,
                Opaque = false,
                BackgroundColor = Theme.White
            };
            webView.ScrollView.Bounces = false;
            webView.ScrollView.BouncesZoom = false;
            webView.TranslatesAutoresizingMaskIntoConstraints = false;
            ContainerView.AddSubview(webView);
            ContainerView.AddConstraints(new[]
            {
                webViewHeightConstraint = NSLayoutConstraint.Create(webView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 0f),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, spinner, NSLayoutAttribute.Bottom, 1f, VerticalMargin),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Top, NSLayoutRelation.GreaterThanOrEqual, ContainerView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, 0f)
            });
        }

        #region IWKNavigationDelegate

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        void DecidePolicy(WKWebView wkWebView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            var policy = viewControllerWeakReference.Unwrap()?.DecidePolicyForNavigationAction(navigationAction) ?? WKNavigationActionPolicy.Cancel;
            decisionHandler(policy);
        }

        #endregion

        #region IWKScriptMessageHandler

        [Export("userContentController:didReceiveScriptMessage:")]
        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            var responseDict = message.Body as NSDictionary;
            if (responseDict == null)
                return;

            var domLoadedNumber = responseDict["domLoaded"] as NSNumber;
            var justLoadedNumber = responseDict["justLoaded"] as NSNumber;
            var resizedNumber = responseDict["resized"] as NSNumber;

            var justLoaded = justLoadedNumber != null && justLoadedNumber.BoolValue;
            var resized = resizedNumber != null && resizedNumber.BoolValue;
            var domLoaded = domLoadedNumber != null && domLoadedNumber.BoolValue;

            Action<CancellationToken, WKWebView, UIActivityIndicatorView, NSLayoutConstraint> resizeAction = null;
            resizeAction = (ct, wv, sv, nslc) => DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(100)), () =>
            {
                if (ct.IsCancellationRequested)
                    return;

                if (wv.IsLoading)
                {
                    resizeAction(ct, wv, sv, nslc);
                }
                else if (nslc.Constant != wv.ScrollView.ContentSize.Height && wv.ScrollView.ContentSize.Height > 1)
                {
                    sv.RemoveFromSuperview();

                    nslc.Constant = wv.ScrollView.ContentSize.Height;
                    SetNeedsLayout();
                }
            });

            Action<CancellationToken, WKWebView, UIActivityIndicatorView, NSLayoutConstraint> stopLoadingAction = null;
            stopLoadingAction = (ct, wv, sv, nslc) =>
            DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(3500)), () =>
            {
                if (ct.IsCancellationRequested)
                    return;

                if (wv.IsLoading)
                {
                    wv.StopLoading();
                    resizeAction(ct, wv, sv, nslc);
                }
            });

            if (domLoaded)
            {
                stopLoadingAction(cts.Token, webView, spinner, webViewHeightConstraint);
            }
            else if (justLoaded || resized)
            {
                resizeAction(cts.Token, webView, spinner, webViewHeightConstraint);
            }
        }

        #endregion

        #region DocumentSubView

        public override void RefreshView()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            if (Document == null)
                return;

            CreateWebView();

            if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
                SetContent(ContentType.PlainText, Document.PlainTextBody);
            else if (!string.IsNullOrWhiteSpace(Document.HtmlBody))
                SetContent(ContentType.Html, Document.HtmlBody);
            else
                SetContent(ContentType.PlainText, Document.PlainTextBody);
        }

        public override void UpdateVisibility()
        {
            if (Document == null)
            {
                Hidden = true;
                return;
            }

            Hidden = false;
        }

        #endregion

        #region Private methods

        void SetContent(ContentType contentType, string content)
        {
            Action<CancellationToken, WKWebView> setContentAction;
            setContentAction = (ct, wv) => DispatchQueue.MainQueue.DispatchAsync(async () =>
             {
                 if (ct.IsCancellationRequested)
                     return;

                 wv.StopLoading();

                 await wv.EvaluateJavaScriptAsync("");

                 if (content == null)
                 {
                     wv.LoadData(NSData.FromString("Content could not be loaded."), "text/plain", "UTF-8", new NSUrl("/"));
                     return;
                 }

                 switch (contentType)
                 {
                     case ContentType.Html:
                         try
                         {
                             HtmlNode headNode = null;

                             var htmlDoc = new HtmlDocument();
                             htmlDoc.LoadHtml(content);

                             foreach (var childNode1 in htmlDoc.DocumentNode.ChildNodes)
                             {
                                 if (headNode != null)
                                     break;

                                 if (childNode1.Name == "head")
                                 {
                                     headNode = childNode1;
                                     break;
                                 }

                                 foreach (var childNode2 in childNode1.ChildNodes)
                                     if (childNode2.Name == "head")
                                     {
                                         headNode = childNode2;
                                         break;
                                     }
                             }

                             if (htmlDoc != null && headNode != null)
                             {
                                 var metaElement = htmlDoc.CreateElement("meta");
                                 metaElement.SetAttributeValue("name", "viewport");
                                 metaElement.SetAttributeValue("content", "initial-scale=0.75, minimum-scale=0.5, maximum-scale=3");//=device-width, shrink-to-fit=yes");// $"initial-scale=0.75, minimum-scale=0.5, maximum-scale=2");
                                 headNode.AppendChild(metaElement);
                                 content = htmlDoc.DocumentNode.OuterHtml;
                             }
                         }
                         catch (Exception ex)
                         {
                             CommonConfig.Logger.Warning("Could not process and insert viewport tag", ex);
                         }

                         wv.LoadHtmlString(content, null);
                         break;
                     case ContentType.PlainText:
                         wv.LoadData(NSData.FromString(content), "text/plain", "UTF-8", new NSUrl("/"));
                         break;
                     default:
                         wv.LoadData(NSData.FromString(string.Empty), "text/plain", "UTF-8", new NSUrl("/"));
                         break;
                 }
             });

            setContentAction(cts.Token, webView);
        }

        #endregion
    }
}