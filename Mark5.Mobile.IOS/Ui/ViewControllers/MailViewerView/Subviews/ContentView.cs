using System;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using HtmlAgilityPack;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class ContentView : MailViewerSubview, IWKNavigationDelegate, IWKScriptMessageHandler
    {
        static readonly NSString script1 = new NSString("window.onload = function () {window.webkit.messageHandlers.sizeNotification.postMessage({justLoaded:true});};");
        static readonly NSString script2 = new NSString("window.onresize = function () {window.webkit.messageHandlers.sizeNotification.postMessage({resized:true});};");
        static readonly NSString script3 = new NSString("document.addEventListener(\"DOMContentLoaded\", function () {window.webkit.messageHandlers.sizeNotification.postMessage({domLoaded:true});});");

        readonly WeakReference<MailViewerViewController> mailViewerViewControllerWeakReference;

        UIActivityIndicatorView spinner;
        WKUserContentController userContentController;
        WKWebViewConfiguration configuration;
        WKWebView webView;
        NSLayoutConstraint webViewHeightConstraint;

        public ContentView(MailViewerViewController mailViewerViewController)
        {
            mailViewerViewControllerWeakReference = mailViewerViewController.Wrap();
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
                UserContentController = userContentController,
                Preferences = preferences,
                SuppressesIncrementalRendering = true,
                AllowsInlineMediaPlayback = false
            };
            webView = new WKWebView(CGRect.Empty, configuration)
            {
                BackgroundColor = Theme.White,
                NavigationDelegate = this,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            webView.ScrollView.Bounces = false;
            webView.ScrollView.BouncesZoom = false;
            ContainerView.AddSubview(webView);
            ContainerView.AddConstraints(new[]
            {
                webViewHeightConstraint = NSLayoutConstraint.Create(webView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 1f),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, spinner, NSLayoutAttribute.Bottom, 1f, VerticalMargin),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Top, NSLayoutRelation.GreaterThanOrEqual, ContainerView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(webView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, 0f)
            });
        }

        #region IWKNavigationDelegate

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        public void DecidePolicy(WKWebView wkWebView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            if (navigationAction.NavigationType == WKNavigationType.LinkActivated
                || navigationAction.NavigationType == WKNavigationType.BackForward
                || navigationAction.NavigationType == WKNavigationType.FormSubmitted
                || navigationAction.NavigationType == WKNavigationType.FormResubmitted)
            {
                if (navigationAction.Request.Url.Scheme == "mailto")
                {
                    var address = navigationAction.Request.Url.ResourceSpecifier;
                    mailViewerViewControllerWeakReference.Unwrap()?.OpenComposeDocumentView(new string[] { address });
                }
                else
                    Integration.OpenLink(navigationAction.Request.Url);

                decisionHandler(WKNavigationActionPolicy.Cancel);
            }
            else if (navigationAction.NavigationType == WKNavigationType.Reload)
                decisionHandler(WKNavigationActionPolicy.Cancel);
            else
                decisionHandler(WKNavigationActionPolicy.Allow);
        }

        #endregion

        #region IWKScriptMessageHandler

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

            Action<WKWebView, UIActivityIndicatorView, NSLayoutConstraint> resizeAction = null;
            resizeAction = (wv, sv, nslc) => DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(100)), () =>
            {
                if (wv.IsLoading)
                {
                    resizeAction(wv, sv, nslc);
                }
                else if (nslc.Constant != wv.ScrollView.ContentSize.Height)
                {
                    sv.RemoveFromSuperview();

                    nslc.Constant = wv.ScrollView.ContentSize.Height;
                    SetNeedsLayout();
                }
            });

            Action<WKWebView, UIActivityIndicatorView, NSLayoutConstraint> stopLoadingAction = null;
            stopLoadingAction = (wv, sv, nslc) => DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(3500)), () =>
             {
                 if (wv.IsLoading)
                 {
                     wv.StopLoading();
                     resizeAction(wv, sv, nslc);
                 }
             });

            if (domLoaded)
            {
                stopLoadingAction(webView, spinner, webViewHeightConstraint);
            }
            else if (justLoaded || resized)
            {
                resizeAction(webView, spinner, webViewHeightConstraint);
            }
        }

        #endregion

        #region DocumentSubView

        public override void RefreshView()
        {
            if (MailMessage == null)
                return;

            CreateWebView();
            SetContent(MailMessage.BodyPlainText, MailMessage.BodyHtmlText);
        }

        public override void UpdateVisibility()
        {
            if (MailMessage == null)
            {
                Hidden = true;
                return;
            }

            Hidden = false;
        }

        #endregion

        #region Private methods

        void SetContent(string plain, string html)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                webView.StopLoading();

                if (plain == null && html == null)
                {
                    webView.LoadData(NSData.FromString("Content could not be loaded."), "text/plain", "UTF-8", new NSUrl("/"));
                    return;
                }

                if (!string.IsNullOrWhiteSpace(html))
                {
                    try
                    {
                        HtmlNode headNode = null;

                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(html);

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
                            html = htmlDoc.DocumentNode.OuterHtml;
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Warning("Could not process and insert viewport tag", ex);
                    }

                    webView.LoadHtmlString(html, null);
                }
                else
                {
                    webView.LoadData(NSData.FromString(plain), "text/plain", "UTF-8", new NSUrl("/"));
                }
            });
        }

        #endregion

    }
}