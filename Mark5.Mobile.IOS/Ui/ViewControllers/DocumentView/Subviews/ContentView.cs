using System;
using System.Threading;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using HtmlAgilityPack;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class ContentView : DocumentSubView, IWKNavigationDelegate, IWKScriptMessageHandler
    {
        static readonly NSString script1 = new NSString("window.onload = function () {window.webkit.messageHandlers.sizeNotification.postMessage({justLoaded:true});};");

        static readonly NSString script2 = new NSString("window.onresize = function () {window.webkit.messageHandlers.sizeNotification.postMessage({resized:true});};");

        static readonly NSString script3 = new NSString("document.addEventListener(\"DOMContentLoaded\", function () {window.webkit.messageHandlers.sizeNotification.postMessage({domLoaded:true});});");

        WKWebView webView;
        NSLayoutConstraint webViewHeightConstraint;

        Func<WKNavigationAction, WKNavigationActionPolicy> navigationActionDelegate;

        UIActivityIndicatorView spinner;

        CancellationTokenSource cts;

        public ContentView(Func<WKNavigationAction, WKNavigationActionPolicy> navigationActionDelegate)
        {
            this.navigationActionDelegate = navigationActionDelegate;
        }

        void CreateWebView()
        {
            if (spinner != null)
            {
                spinner.RemoveFromSuperview();
            }

            spinner = null;

            spinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            spinner.TranslatesAutoresizingMaskIntoConstraints = false;
            spinner.HidesWhenStopped = true;
            ContainerView.AddSubview(spinner);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(spinner, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(spinner, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
            });

            spinner.StartAnimating();

            if (webView != null)
            {
                webView.RemoveFromSuperview();
                webView.NavigationDelegate = null;
            }

            webView = null;

            var preferences = new WKPreferences();
            preferences.MinimumFontSize = 12f;
            preferences.JavaScriptCanOpenWindowsAutomatically = false;
            preferences.JavaScriptEnabled = true;

            var wkscript1 = new WKUserScript(script1, WKUserScriptInjectionTime.AtDocumentEnd, true);
            var wkscript2 = new WKUserScript(script2, WKUserScriptInjectionTime.AtDocumentEnd, true);
            var wkscript3 = new WKUserScript(script3, WKUserScriptInjectionTime.AtDocumentStart, true);

            var userContentController = new WKUserContentController();
            userContentController.AddUserScript(wkscript1);
            userContentController.AddUserScript(wkscript2);
            userContentController.AddUserScript(wkscript3);
            userContentController.AddScriptMessageHandler(this, "sizeNotification");

            var configuration = new WKWebViewConfiguration();
            configuration.SuppressesIncrementalRendering = true;
            configuration.AllowsInlineMediaPlayback = false;
            configuration.UserContentController = userContentController;
            configuration.Preferences = preferences;
            configuration.DataDetectorTypes = WKDataDetectorTypes.PhoneNumber | WKDataDetectorTypes.Link;

            webView = new WKWebView(CGRect.Empty, configuration);
            webView.NavigationDelegate = this;
            webView.Opaque = false;
            webView.BackgroundColor = UIColor.White;
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
            decisionHandler(navigationActionDelegate(navigationAction));
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

            Action<CancellationToken, WKWebView, UIActivityIndicatorView, NSLayoutConstraint> resizeAction = null;
            resizeAction = (ct, wv, sv, nslc) => DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(100)),
                () =>
                {
                    if (ct.IsCancellationRequested)
                    {
                        CommonConfig.Logger.Debug($"{wv.GetHashCode()} - CANCELLATION REQUESTED IN RESIZE ACTION ");
                        return;
                    }

                    CommonConfig.Logger.Debug($"{wv.GetHashCode()} - RESIZE ACTION ");

                    if (wv.IsLoading)
                    {
                        CommonConfig.Logger.Debug($"{wv.GetHashCode()} - IS LOADING ");

                        resizeAction(ct, wv, sv, nslc);
                    }
                    else if (nslc.Constant != wv.ScrollView.ContentSize.Height && wv.ScrollView.ContentSize.Height > 1)
                    {
                        CommonConfig.Logger.Debug($"{wv.GetHashCode()} - INCREASING HEIGHT FROM {nslc.Constant} TO {wv.ScrollView.ContentSize.Height} ");
                        sv.RemoveFromSuperview();

                        nslc.Constant = wv.ScrollView.ContentSize.Height;

                        SetNeedsLayout();
                    }
                });

            Action<CancellationToken, WKWebView, UIActivityIndicatorView, NSLayoutConstraint> stopLoadingAction = null;
            stopLoadingAction = (ct, wv, sv, nslc) =>
            DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(3500)),
                                                  () =>
                                                  {
                                                      if (ct.IsCancellationRequested)
                                                      {
                                                          CommonConfig.Logger.Debug($"{wv.GetHashCode()} - CANCELLATION REQUESTED IN SET STOP LOADING ACTION ");
                                                          return;
                                                      }

                                                      CommonConfig.Logger.Debug($"{wv.GetHashCode()} - TIMEOUT ");

                                                      if (wv.IsLoading)
                                                      {
                                                          CommonConfig.Logger.Debug($"{wv.GetHashCode()} - STOP LOADING ");

                                                          wv.StopLoading();
                                                          resizeAction(ct, wv, sv, nslc);
                                                      }
                                                  });

            if (domLoadedNumber != null && domLoadedNumber.BoolValue)
            {
                CommonConfig.Logger.Debug($"{webView.GetHashCode()} - DOM LOADED ");
                stopLoadingAction(cts.Token, webView, spinner, webViewHeightConstraint);
            }
            else if (justLoadedNumber != null && justLoadedNumber.BoolValue
                     || resizedNumber != null && resizedNumber.BoolValue)
            {
                CommonConfig.Logger.Debug($"{webView.GetHashCode()} - JUST_LOADED={justLoadedNumber} / RESIZED={resizedNumber} ");
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

            CommonConfig.Logger.Debug($"{webView.GetHashCode()} -REFRESHING - SUB = {DocumentPreview.Subject}");

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
                 CommonConfig.Logger.Debug($"{wv.GetHashCode()} - SET CONTENT ACTION ");

                 if (ct.IsCancellationRequested)
                 {
                     CommonConfig.Logger.Debug($"{wv.GetHashCode()} - CANCELLATION REQUESTED IN SET CONTENT ");
                     return;
                 }

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
                                 metaElement.SetAttributeValue("content", $"initial-scale=0.75, minimum-scale=0.5, maximum-scale=2");
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
                 CommonConfig.Logger.Debug($"{wv.GetHashCode()} - CONTENT SET");

             });

            setContentAction(cts.Token, webView);
        }

        #endregion
    }
}