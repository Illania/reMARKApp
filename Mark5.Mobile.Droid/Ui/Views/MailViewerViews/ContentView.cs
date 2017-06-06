//
// Project: Mark5.Mobile.Droid
// File: ContentView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Android.Content;
using Android.Views;
using Android.Webkit;

namespace Mark5.Mobile.Droid.Ui.Views.MailViewerViews
{
    public class ContentView : MailViewerView
    {
        CustomWebView webView;

        public ContentView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            SetPadding(DistanceNone, DistanceNormal, DistanceNone, DistanceNormal);

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

        public override void RefreshView()
        {
            if (MailMessage != null && MailMessage != null)
            {
                Visibility = ViewStates.Visible;

                if (!string.IsNullOrWhiteSpace(MailMessage.BodyHtmlText))
                    webView.LoadDataWithBaseURL(null, MailMessage.BodyHtmlText ?? "Content could not be loaded.", "text/html", "UTF-8", null);
                else
                    webView.LoadDataWithBaseURL(null, MailMessage.BodyPlainText ?? "Content could not be loaded.", "text/plain", "UTF-8", null);
            }
            else
            {
                Visibility = ViewStates.Gone;

                webView.LoadData(string.Empty, "text/plain", "UTF-8");
            }
        }

        class CustomWebView : WebView
        {
            public CustomWebView(Context context)
                : base(context)
            {
            }

            public override bool OnTouchEvent(MotionEvent e)
            {
                if (e.FindPointerIndex(0) != -1)
                    RequestDisallowInterceptTouchEvent(e.PointerCount > 1);

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
            [Obsolete]
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                view.Context.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
                return true;
            }
        }
    }
}