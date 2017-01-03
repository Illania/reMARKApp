//
// Project: Mark5.Mobile.IOS
// File: ContentView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class ContentView : DocumentView, IUIWebViewDelegate
    {

        const int ResizeLimit = 10;
        const int MaximumSizeForScalesPagesToFit = 10000;

        bool changeListenerAdded;
        int resizeLimitCounter;

        float defaultHeight;

        UIWebView webView;
        NSLayoutConstraint heightConstraint;

        Selector contentSizeSelector;

        public ContentView()
        {
            defaultHeight = 200.0f; //TODO desktop rendering

            Initialize();
        }

        void Initialize()
        {
            webView = new UIWebView();
            webView.ShouldStartLoad = (webView, request, navigationType) =>
            {
                if (navigationType == UIWebViewNavigationType.LinkClicked)
                {
                    Integration.OpenLink(request.Url);
                }

                return navigationType == UIWebViewNavigationType.Other;
            };
            webView.Opaque = false;
            webView.BackgroundColor = UIColor.White;
            webView.ScrollView.Bounces = false;
            webView.ScrollView.ScrollsToTop = false;
            webView.ScalesPageToFit = false; //TODO desktop rendering
            webView.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(webView);
            heightConstraint = NSLayoutConstraint.Create(webView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, defaultHeight);
            AddConstraints(new[]
                {
                    heightConstraint,
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                });
        }

        public void Recycle()
        {
            //UnregisterChangeListeners();

            Document = null;

            if (webView != null)
            {
                webView.LoadData("", "text/plain", "UTF-8", new NSUrl("/"));
                webView.RemoveFromSuperview();
                webView.Delegate = null;
                webView.WeakDelegate = null;
                webView = null;
            }

            contentSizeSelector = null;

            GC.Collect();
        }

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

            //UnregisterChangeListeners();
            //RegisterChangeListeners();

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

            //webView.ScalesPageToFit = Settings.DesktopRendering;
            //UnregisterChangeListeners();
            //RegisterChangeListeners();

            heightConstraint.Constant = defaultHeight;

            SetContent(ContentType.PlainText, string.Empty);
        }


        #endregion

    }

}
