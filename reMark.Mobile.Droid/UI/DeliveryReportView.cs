using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Webkit;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Reports;
using reMark.Mobile.Droid.Model;
using reMark.Mobile.Droid.Ui.Views.DocumentViews;
using reMark.Mobile.Droid.Utilities.Extensions;
using WebView = Android.Webkit.WebView;

namespace reMark.Mobile.Droid.Ui
{
    public class DeliveryReportView: DocumentView
    {
        private string referenceNumber;
        private TransmitDestination transmitDestination;

        CustomWebView webView;

        public DeliveryReportView(Context context)
            : base(context)
        {
            InitializeView();
        }

        public void SetData(string referenceNumber, TransmitDestination transmitDestination)
        {
            this.referenceNumber = referenceNumber;
            this.transmitDestination = transmitDestination;
        }


        void InitializeView()
        {
            SetPadding(DistanceNone, DistanceNormal, DistanceNone, DistanceNormal);

            var customWebViewClient = new WebViewClient();
            webView = new CustomWebView(Context);
            webView.SetWebViewClient(customWebViewClient);
            webView.Settings.SetSupportZoom(true);
            webView.Settings.UseWideViewPort = true;
            webView.Settings.LoadWithOverviewMode = true;
            webView.SetInitialScale(1);
            webView.Settings.BuiltInZoomControls = true;
            webView.Settings.DisplayZoomControls = false;
            webView.Settings.JavaScriptEnabled = false;
            webView.VerticalScrollBarEnabled = false;
            webView.HorizontalScrollBarEnabled = false;
            AddView(webView);
        }

        public override async Task RefreshView()
        {

            var html = DeliveryStatusReport.GetReportHtml(new DeliveryReportData(referenceNumber,
                  transmitDestination.Address,
                  transmitDestination.Status));

            Visibility = ViewStates.Visible;
            await webView.LoadHtml(Context, html, HtmlProcessingConfiguration.DefaultForViewing); ;

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


    }
}
