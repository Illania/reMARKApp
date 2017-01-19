//
// Project: Mark5.Mobile.IOS
// File: ContentView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class ContentView : DocumentSubView, IWKNavigationDelegate, IUIScrollViewDelegate, IUIGestureRecognizerDelegate
    {
        float defaultHeight = 1.0f;
        float defaultWidth = 1.0f;

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
        public NSLayoutConstraint ExternalInitialWidthConstraint;
        public NSLayoutConstraint ExternalWidthConstraint;
        public NSLayoutConstraint ExternalRightConstraint;

        public ContentView(UIScrollView mainScroll)
        {
            mainScrollView = mainScroll;
            Initialize();
        }

        void Initialize()
        {
            var preferences = new WKPreferences();
            preferences.JavaScriptCanOpenWindowsAutomatically = false;
            preferences.JavaScriptEnabled = true;

            //TODO disable link clicking (or open external browser)
            var configuration = new WKWebViewConfiguration();
            configuration.Preferences = preferences;

            webView = new WKWebView(CoreGraphics.CGRect.Empty, configuration);
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
            heightConstraint = NSLayoutConstraint.Create(webView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, defaultHeight);
            widthConstraint = NSLayoutConstraint.Create(webView, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, defaultWidth);
            ContainerView.AddConstraints(new[]
                {
                    heightConstraint,
                    //widthConstraint,
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin),
                    NSLayoutConstraint.Create(webView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                });
        }

        #region IWKNavigationDelegate

        [Export("webView:didFinishNavigation:")]
        void DidFinishNavigation(WKWebView wkWebView, WKNavigation navigation)
        {
            BeginInvokeOnMainThread(async () =>
            {
                //Not sure why it does not work withouth the following line
                await webView.EvaluateJavaScriptAsync("");

                initialHeight = webView.ScrollView.ContentSize.Height;
                initialZoom = webView.ScrollView.ZoomScale;
                initialWidth = webView.ScrollView.ContentSize.Width;

                heightConstraint.Constant = initialHeight;
                widthConstraint.Constant = initialWidth;
            });
        }

        #endregion

        #region IUIScrollViewDelegate

        [Export("scrollViewWillBeginZooming:withView:")]
        public void ZoomingStarted(UIScrollView scrollView, UIView view)
        {
            webView.RemoveConstraint(widthConstraint);
            //mainScrollView.RemoveConstraint(ExternalWidthConstraint);
            //mainScrollView.RemoveConstraint(ExternalRightConstraint);
            actualZoomScaleBeforeZooming = scrollView.ZoomScale;

            zomingStarted = true;
        }

        bool zomingStarted;

        [Export("scrollViewDidZoom:")]
        public void DidZoom(UIScrollView scrollView)
        {
            if (!zomingStarted)
            {
                return;
            }

            widthConstraint.Constant = (initialWidth / initialZoom) * scrollView.ZoomScale;
            ExternalWidthConstraint.Constant = widthConstraint.Constant;

            var zoomScaleRatio = scrollView.ZoomScale / actualZoomScaleBeforeZooming;


            var scrollViewOffset = mainScrollView.ContentOffset;
            scrollViewOffset.X += centerGestureStartX * (zoomScaleRatio - 1);
            scrollViewOffset.Y += centerGestureStartY * (zoomScaleRatio - 1);

            mainScrollView.ContentOffset = scrollViewOffset;

            actualZoomScaleBeforeZooming = scrollView.ZoomScale;
        }

        [Export("scrollViewDidEndZooming:withView:atScale:")]
        public void ZoomingEnded(UIScrollView scrollView, UIView withView, nfloat atScale)
        {
            BeginInvokeOnMainThread(() =>
           {
               var zoomScaleRatio = scrollView.ZoomScale / actualZoomScaleBeforeZooming;

               heightConstraint.Constant = (initialHeight / initialZoom) * scrollView.ZoomScale;
               widthConstraint.Constant = (initialWidth / initialZoom) * scrollView.ZoomScale;
               ExternalWidthConstraint.Constant = widthConstraint.Constant;

               var scrollViewOffset = mainScrollView.ContentOffset;
               scrollViewOffset.X += centerGestureStartX * (zoomScaleRatio - 1);
               scrollViewOffset.Y += centerGestureStartY * (zoomScaleRatio - 1);

               mainScrollView.ContentOffset = scrollViewOffset;

               //webView.AddConstraint(widthConstraint);
               //mainScrollView.AddConstraint(ExternalWidthConstraint);
               //mainScrollView.AddConstraint(ExternalRightConstraint);
           });
        }

        #endregion

        public void SetMinimumScale()
        {
            //webView.ScrollView.MinimumZoomScale = initialZoom;
        }

        #region IUIGestureRecognizerDelegate

        [Export("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
        public bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }

        #endregion

        #region Gesture Recognizer handlers

        void HandlePinch(UIPinchGestureRecognizer gestureRecognizer) //TODO can be improved
        {
            if (gestureRecognizer.State == UIGestureRecognizerState.Began)
            {
                centerGestureStartX = gestureRecognizer.LocationInView(webView.ScrollView).X;
                centerGestureStartY = gestureRecognizer.LocationInView(webView.ScrollView).Y;
            }
        }

        void HandleTap(UITapGestureRecognizer gestureRecognizer)
        {
            centerGestureStartX = gestureRecognizer.LocationInView(webView.ScrollView).X; //TODO works worse than the pinch
            centerGestureStartY = gestureRecognizer.LocationInView(webView.ScrollView).Y;
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
            widthConstraint.Constant = defaultWidth;

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

            SetContent(ContentType.PlainText, string.Empty);
        }

        #endregion

    }

}
