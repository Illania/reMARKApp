//
// Project: Mark5.Mobile.Droid
// File: ContentView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Mark5.Mobile.Common.Model;
using Android.Net;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class ContentView : LinearLayoutCompat, IDocumentView
    {

        public DocumentPreview DocumentPreview
        {
            get;
            set;
        }

        public Document Document
        {
            get;
            set;
        }

        CustomWebView webView;

        public ContentView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            var paddingSmall = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 8.0f, Resources.DisplayMetrics) + 0.5f);
            SetPadding(0, paddingSmall, 0, paddingSmall);

            webView = new CustomWebView(Context);
            webView.SetWebViewClient(new CustomWebViewClient());
            webView.Settings.SetSupportZoom(true);
            webView.Settings.BuiltInZoomControls = true;
            webView.Settings.DisplayZoomControls = false;
            webView.Settings.JavaScriptEnabled = false;
            webView.VerticalScrollBarEnabled = false;
            webView.HorizontalScrollBarEnabled = false;
            AddView(webView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        public void RefreshView()
        {
            if (DocumentPreview != null && Document != null)
            {
                Visibility = ViewStates.Visible;

                webView.LoadData(Document.HtmlBody, "text/html", "UTF-8");
            }
            else
            {
                Visibility = ViewStates.Gone;

                webView.LoadData(string.Empty, "text/plain", "UTF-8");
            }

            Invalidate();
            RequestLayout();
        }

        class CustomWebView : WebView
        {

            public CustomWebView(Context context)
                : base(context)
            {
            }

            public override bool OnTouchEvent(MotionEvent e)
            {
                if (MotionEventCompat.FindPointerIndex(e, 0) != -1)
                {
                    RequestDisallowInterceptTouchEvent(e.PointerCount > 1);
                }

                return base.OnTouchEvent(e);
            }

            protected override void OnOverScrolled(int scrollX, int scrollY, bool clampedX, bool clampedY)
            {
                base.OnOverScrolled(scrollX, scrollY, clampedX, clampedY);
                RequestDisallowInterceptTouchEvent(true);
            }
        }

        class CustomWebViewClient : WebViewClient
        {

            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                view.Context.StartActivity(new Intent(Intent.ActionView, Uri.Parse(url)));
                return true;
            }
        }
    }
}
