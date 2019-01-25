using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Webkit;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.IOS.Utilities.Extensions;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{
    public class ContentView : DocumentView
    {
        CustomWebView webView;

        public event EventHandler<string> MailToLinkClicked = delegate { };

        public ContentView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            SetPadding(DistanceNone, DistanceNormal, DistanceNone, DistanceNormal);

            var customWebViewClient = new CustomWebViewClient();
            customWebViewClient.MailToLinkClicked += MailToLinkClicked;

            webView = new CustomWebView(Context);
            webView.SetWebViewClient(customWebViewClient);
            webView.Settings.SetSupportZoom(true);
            webView.Settings.BuiltInZoomControls = true;
            webView.Settings.DisplayZoomControls = false;
            webView.Settings.JavaScriptEnabled = false;
            webView.VerticalScrollBarEnabled = false;
            webView.HorizontalScrollBarEnabled = false;
            AddView(webView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        public override async Task RefreshView()
        {
            if (DocumentPreview != null && Document != null)
            {
                Visibility = ViewStates.Visible;

                if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
                    await webView.LoadPlainText(Context, Document.PlainTextBody, PlainTextProcessingConfiguration.DefaultForViewing);
                else if (!string.IsNullOrWhiteSpace(Document.HtmlBody))
                    await webView.LoadHtml(Context, Document.HtmlBody, HtmlProcessingConfiguration.DefaultForViewing);
                else if (!string.IsNullOrWhiteSpace(Document.PlainTextBody))
                    await webView.LoadPlainText(Context, Document.PlainTextBody, PlainTextProcessingConfiguration.DefaultForViewing);
                else
                    webView.LoadNoContentString(Context);
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
            public event EventHandler<string> MailToLinkClicked = delegate { };

            [Obsolete]
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                if (url.StartsWith("mailto", StringComparison.InvariantCultureIgnoreCase))
                    MailToLinkClicked(this, url);
                else
                    view.Context.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
                return true;
            }
        }
    }
}