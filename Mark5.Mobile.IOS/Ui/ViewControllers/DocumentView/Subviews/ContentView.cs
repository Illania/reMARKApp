//
// Project: Mark5.Mobile.IOS
// File: ContentView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class ContentView : DocumentSubView, IWKNavigationDelegate
    {
        float defaultHeight = 200.0f;

        WKWebView webView;
        NSLayoutConstraint heightConstraint;

        public ContentView()
        {
            Initialize();
        }

        void Initialize()
        {
            var preferences = new WKPreferences();
            preferences.JavaScriptCanOpenWindowsAutomatically = false;
            preferences.JavaScriptEnabled = true;

            var configuration = new WKWebViewConfiguration();
            configuration.Preferences = preferences;

            webView = new WKWebView(CoreGraphics.CGRect.Empty, configuration);
            webView.NavigationDelegate = this; //TODO talk with Bartosz about this (no difference between this and a weak delegate)
            webView.Opaque = false;
            webView.BackgroundColor = UIColor.White;
            webView.ScrollView.Bounces = false;
            webView.ScrollView.ScrollsToTop = false;
            webView.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(webView);
            heightConstraint = NSLayoutConstraint.Create(webView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, defaultHeight);
            AddConstraints(new[]
                {
                    heightConstraint,
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                });
        }

        #region IWKNavigationDelegate

        [Export("webView:didFinishNavigation:")]
        void DidFinishNavigation(WKWebView wkWebView, WKNavigation navigation)
        {
            BeginInvokeOnMainThread(async () =>
            {
                //TODO No idea about why the following line is necessary (withouth it works strangely)
                await webView.EvaluateJavaScriptAsync("");
                heightConstraint.Constant = webView.ScrollView.ContentSize.Height;
                SetNeedsLayout();
            });
        }

        #endregion

        #region DocumentSubView

        public override void RefreshView()
        {
            if (Document == null)
            {
                Clear();
            }

            if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
            {
                SetContent(ContentType.PlainText, Document.PlainTextBody);
            }
            else if (!string.IsNullOrWhiteSpace(Document.HtmlBody))
            {
                SetContent(ContentType.Html, Document.HtmlBody);
            }
            else
            {
                SetContent(ContentType.PlainText, Document.PlainTextBody);
            }
        }

        public override void UpdateVisibility()
        {
            Hidden = false; //TODO should be from preferences
        }

        #endregion

        #region Private methods

        void SetContent(ContentType contentType, string content)
        {
            webView.StopLoading();
            heightConstraint.Constant = defaultHeight;

            switch (contentType)
            {
                case ContentType.Html:
                    webView.LoadHtmlString(content, null);
                    break;
                case ContentType.PlainText:
                    webView.LoadData(NSData.FromString(content), "text/plain", "UTF-8", new NSUrl("/"));
                    break;
                default:
                    webView.LoadData(NSData.FromString(string.Empty), "text/plain", "UTF-8", new NSUrl("/"));
                    break;
            }
        }

        void Clear()
        {
            webView.StopLoading();
            heightConstraint.Constant = defaultHeight;

            SetContent(ContentType.PlainText, string.Empty);
        }


        #endregion

    }

}
