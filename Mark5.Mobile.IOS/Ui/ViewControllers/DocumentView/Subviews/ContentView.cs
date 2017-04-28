//
// Project: Mark5.Mobile.IOS
// File: ContentView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
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

        readonly static NSString script1 = new NSString("window.onload = function () {window.webkit.messageHandlers.sizeNotification.postMessage({justLoaded:true});};");
        readonly static NSString script2 = new NSString("window.onresize = function () {window.webkit.messageHandlers.sizeNotification.postMessage({resized:true});};");

        WKWebView webView;
        NSLayoutConstraint webViewHeightConstraint;

        Func<WKNavigationAction, WKNavigationActionPolicy> navigationActionDelegate;

        public ContentView(Func<WKNavigationAction, WKNavigationActionPolicy> navigationActionDelegate)
        {
            this.navigationActionDelegate = navigationActionDelegate;
        }

        void CreateWebView()
        {
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

            var userContentController = new WKUserContentController();
            userContentController.AddUserScript(wkscript1);
            userContentController.AddUserScript(wkscript2);
            userContentController.AddScriptMessageHandler(this, "sizeNotification");

            var configuration = new WKWebViewConfiguration();
            configuration.SuppressesIncrementalRendering = true;
            configuration.AllowsInlineMediaPlayback = false;
            configuration.UserContentController = userContentController;
            configuration.Preferences = preferences;

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
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, 0f),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, 0f)
                });
        }

        #region IWKNavigationDelegate

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        void DecidePolicy(WKWebView wkWebView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler) => decisionHandler(navigationActionDelegate(navigationAction));

        #endregion

        #region IWKScriptMessageHandler

        [Export("userContentController:didReceiveScriptMessage:")]
        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            var responseDict = message.Body as NSDictionary;
            if (responseDict == null)
                return;

            var justLoadedNumber = responseDict["justLoaded"] as NSNumber;
            var resizedNumber = responseDict["resized"] as NSNumber;

            if ((justLoadedNumber != null && justLoadedNumber.BoolValue) || (resizedNumber != null && resizedNumber.BoolValue))
            {
                Action<WKWebView, NSLayoutConstraint> resizeAction = null;
                resizeAction = (wv, nslc) => DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(100)), () =>
                {
                    if (wv.IsLoading)
                    {

                        resizeAction(wv, nslc);
                    }
                    else if (nslc.Constant != wv.ScrollView.ContentSize.Height)
                    {
                        nslc.Constant = wv.ScrollView.ContentSize.Height;

                        SetNeedsLayout();
                    }
                });
                resizeAction(webView, webViewHeightConstraint);
            }
        }

        #endregion

        #region DocumentSubView

        public override void RefreshView()
        {
            if (Document == null)
            {
                return;
            }

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
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                webView.StopLoading();

                if (content == null)
                {
                    webView.LoadData(NSData.FromString("Content could not be loaded."), "text/plain", "UTF-8", new NSUrl("/"));
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

                        webView.LoadHtmlString(content, null);
                        break;
                    case ContentType.PlainText:
                        webView.LoadData(NSData.FromString(content), "text/plain", "UTF-8", new NSUrl("/"));
                        break;
                    default:
                        webView.LoadData(NSData.FromString(string.Empty), "text/plain", "UTF-8", new NSUrl("/"));
                        break;
                }
            });
        }

        #endregion

    }

}
