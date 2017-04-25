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
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class ContentView : DocumentSubView, IWKNavigationDelegate, IUIScrollViewDelegate, IUIGestureRecognizerDelegate
    {
        float defaultHeight = 1f;
        float defaultWidth = 1f;

        WKWebView webView;
        NSLayoutConstraint heightConstraint;
        NSLayoutConstraint widthConstraint;

        nfloat initialHeight;
        nfloat initialWidth;
        nfloat initialZoom;

        nfloat centerGestureStartX;
        nfloat centerGestureStartY;
        nfloat actualZoomScaleBeforeZooming;

        UIScrollView mainScrollView;

        IDisposable observer;

        Func<WKNavigationAction, WKNavigationActionPolicy> navigationActionDelegate;

        bool zoomingStarted;

        public ContentView(UIScrollView mainScrollView, Func<WKNavigationAction, WKNavigationActionPolicy> navigationActionDelegate)
        {
            this.mainScrollView = mainScrollView;
            this.navigationActionDelegate = navigationActionDelegate;
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
            observer = webView.ScrollView.AddObserver("contentSize", NSKeyValueObservingOptions.New, HandleWebViewContentSizeChanged);
            webView.NavigationDelegate = this;
            webView.ScrollView.Delegate = this;
            webView.Opaque = false;
            webView.BackgroundColor = UIColor.White;
            webView.ScrollView.Bounces = false;
            webView.ScrollView.ScrollsToTop = false;
            webView.ScrollView.ScrollEnabled = false;
            webView.TranslatesAutoresizingMaskIntoConstraints = false;
            webView.ScrollView.UserInteractionEnabled = true;
            var pinchRecognizer = new UIPinchGestureRecognizer(HandlePinch);
            pinchRecognizer.Delegate = this;
            var tapRecognizer = new UITapGestureRecognizer(HandleTap);
            tapRecognizer.Delegate = this;
            webView.ScrollView.AddGestureRecognizer(pinchRecognizer);
            webView.ScrollView.AddGestureRecognizer(tapRecognizer);
            ContainerView.AddSubview(webView);
            heightConstraint = NSLayoutConstraint.Create(webView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, defaultHeight);
            widthConstraint = NSLayoutConstraint.Create(webView, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, defaultWidth);
            ContainerView.AddConstraints(new[]
                {
                    heightConstraint,
                    widthConstraint,
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin)
                });
        }

        protected override void Dispose(bool disposing)
        {
            observer.Dispose();
            base.Dispose(disposing);
        }

        #region Observer handler

        void HandleWebViewContentSizeChanged(NSObservedChange obj)
        {
            if (webView.ScrollView.Zooming)
            {
                return;
            }

            BeginInvokeOnMainThread(() =>
           {
               initialHeight = webView.ScrollView.ContentSize.Height;
               initialZoom = webView.ScrollView.ZoomScale;
               initialWidth = webView.ScrollView.ContentSize.Width;

               heightConstraint.Constant = initialHeight;
               widthConstraint.Constant = initialWidth;
           });
        }

        #endregion

        #region IWKNavigationDelegate

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        void DecidePolicy(WKWebView wkWebView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            decisionHandler(navigationActionDelegate(navigationAction));
        }

        #endregion

        #region IUIScrollViewDelegate

        [Export("scrollViewWillBeginZooming:withView:")]
        public void ZoomingStarted(UIScrollView scrollView, UIView view)
        {
            actualZoomScaleBeforeZooming = scrollView.ZoomScale;
            zoomingStarted = true;
        }

        [Export("scrollViewDidZoom:")]
        public void DidZoom(UIScrollView scrollView)
        {
            if (!zoomingStarted)
            {
                return;
            }


            //TODO Note: the scrollView does not scroll back after reaching a zoom scale of 5, that is the default 
            //maximum zoom scale. Trying to change the maximum zoom scale does not work, because it gets modified internally anyway

            var zoomScaleRatio = scrollView.ZoomScale / actualZoomScaleBeforeZooming;

            heightConstraint.Constant = (initialHeight / initialZoom) * scrollView.ZoomScale;
            widthConstraint.Constant = (initialWidth / initialZoom) * scrollView.ZoomScale;

            var scrollViewOffset = mainScrollView.ContentOffset;
            scrollViewOffset.X += centerGestureStartX * (zoomScaleRatio - 1);
            scrollViewOffset.Y += centerGestureStartY * (zoomScaleRatio - 1);

            mainScrollView.ContentOffset = scrollViewOffset;

            actualZoomScaleBeforeZooming = scrollView.ZoomScale;

            var webViewScrollViewOffset = scrollView.ContentOffset;
            webViewScrollViewOffset.X = 0;
            webViewScrollViewOffset.Y = 0;
            scrollView.ContentOffset = webViewScrollViewOffset;
        }

        #endregion

        #region IUIGestureRecognizerDelegate

        [Export("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
        public bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }

        #endregion

        #region Gesture Recognizer handlers

        void HandlePinch(UIPinchGestureRecognizer gestureRecognizer)
        {
            if (gestureRecognizer.State == UIGestureRecognizerState.Began)
            {
                centerGestureStartX = gestureRecognizer.LocationInView(webView.ScrollView).X;
                centerGestureStartY = gestureRecognizer.LocationInView(webView.ScrollView).Y;
            }
        }

        void HandleTap(UITapGestureRecognizer gestureRecognizer)
        {
            centerGestureStartX = gestureRecognizer.LocationInView(webView.ScrollView).X;
            centerGestureStartY = gestureRecognizer.LocationInView(webView.ScrollView).Y;
        }

        #endregion

        #region DocumentSubView

        public override void RefreshView()
        {
            if (Document == null)
            {
                Clear();
                return;
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
            webView.StopLoading();
            heightConstraint.Constant = defaultHeight;
            widthConstraint.Constant = defaultWidth;

            if (content == null)
            {
                webView.LoadData(NSData.FromString("Content could not be loaded."), "text/plain", "UTF-8", new NSUrl("/"));
                return;
            }

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
            widthConstraint.Constant = defaultWidth;
            initialHeight = 0;
            initialWidth = 0;
            initialZoom = 0;

            SetContent(ContentType.PlainText, string.Empty);
        }

        #endregion

    }

}
