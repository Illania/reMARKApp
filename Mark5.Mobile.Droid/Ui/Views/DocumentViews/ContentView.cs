using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Print;
using Android.Views;
using Android.Webkit;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Droid.Utilities.Extensions;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{
    public class ContentView : DocumentView
    {
        private CustomWebView _webView;

        public event EventHandler<string> MailToLinkClicked = delegate { };

        public ContentView(Context context)
            : base(context)
        {
            InitializeView();
        }

        private void InitializeView()
        {
            SetPadding(DistanceNone, DistanceNormal, DistanceNone, DistanceNormal);

            var customWebViewClient = new CustomWebViewClient();
            customWebViewClient.MailToLinkClicked += (sender, e) => MailToLinkClicked(sender, e);

            _webView = new CustomWebView(Context);
            _webView.SetWebViewClient(customWebViewClient);
            _webView.Settings.SetSupportZoom(true);
            _webView.Settings.BuiltInZoomControls = true;
            _webView.Settings.DisplayZoomControls = false;
            _webView.Settings.JavaScriptEnabled = false;
            _webView.VerticalScrollBarEnabled = false;
            _webView.HorizontalScrollBarEnabled = false;
            AddView(_webView, new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        public override async Task RefreshView()
        {
            if (DocumentPreview != null && Document != null)
            {
                Visibility = ViewStates.Visible;

                if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
                    await _webView.LoadPlainText(Context, Document.PlainTextBody, PlainTextProcessingConfiguration.DefaultForViewing);
                else if (!string.IsNullOrWhiteSpace(Document.HtmlBody))
                    await _webView.LoadHtml(Context, Document.HtmlBody, HtmlProcessingConfiguration.DefaultForViewing);
                else if (!string.IsNullOrWhiteSpace(Document.PlainTextBody))
                    await _webView.LoadPlainText(Context, Document.PlainTextBody, PlainTextProcessingConfiguration.DefaultForViewing);
                else
                    _webView.LoadNoContentString(Context);
            }
            else
            {
                Visibility = ViewStates.Gone;

                _webView.LoadData(string.Empty, "text/plain", "UTF-8");
            }
        }

        internal void Print()
        {
            try
            {
                if (Context == null)
                    return;

                var printManager = (PrintManager)Context.GetSystemService(Context.PrintService);
                var printAdapter = _webView.CreatePrintDocumentAdapter($"print_{DocumentPreview.ReferenceNumber}");
                printManager?.Print("Document", printAdapter, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex.Message);
            }
}

        private class CustomWebView : WebView
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

        private class CustomWebViewClient : WebViewClient
        {
            public event EventHandler<string> MailToLinkClicked = delegate { };

            [Obsolete]
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                if (url.StartsWith("mailto", StringComparison.InvariantCultureIgnoreCase))
                {
                    MailToLinkClicked(this, url);   
                }
                else if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    return true;
                else
                {
                    OpenUrl(view, url);
                }

                return true;
            }

            private static void OpenUrl(WebView view, string url)
            {
                try
                {
                    view.Context?.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Warning(ex.Message);
                }
            }
        }
    }
}