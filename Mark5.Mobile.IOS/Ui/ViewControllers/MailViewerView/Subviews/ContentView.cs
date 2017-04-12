//
// Project: Mark5.Mobile.IOS
// File: ContentView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Foundation;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{

    public class ContentView : MailViewerSubview, IWKNavigationDelegate, IUIScrollViewDelegate, IUIGestureRecognizerDelegate
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

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        void DecidePolicy(WKWebView wkWebView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            decisionHandler(navigationActionDelegate(navigationAction));
        }

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

        [Export("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
        public bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }

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

        public override void RefreshView()
        {
            if (MailMessage == null)
            {
                Clear();
                return;
            }

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

        void SetContent(string plain, string html)
        {
            webView.StopLoading();
            heightConstraint.Constant = defaultHeight;
            widthConstraint.Constant = defaultWidth;

            if (string.IsNullOrWhiteSpace(plain) && string.IsNullOrWhiteSpace(html))
            {
                webView.LoadData(NSData.FromString("Content could not be loaded."), "text/plain", "UTF-8", new NSUrl("/"));
                return;
            }

            if (!string.IsNullOrWhiteSpace(html))
                webView.LoadHtmlString(html, null);
            else
                webView.LoadData(NSData.FromString(plain), "text/plain", "UTF-8", new NSUrl("/"));
        }

        void Clear()
        {
            webView.StopLoading();
            heightConstraint.Constant = defaultHeight;
            widthConstraint.Constant = defaultWidth;
            initialHeight = 0;
            initialWidth = 0;
            initialZoom = 0;

            SetContent(null, null);
        }
    }
}
