using System;
using Android.Content;
using Android.Views;
using Android.Webkit;
using Foundation;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Ui.Views.Common
{
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
        public event EventHandler PageFinishedLoading = delegate { };

        [Obsolete]
        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            view.Context.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
            return true;
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            PageFinishedLoading(this, EventArgs.Empty);
            
            string editorScript;
            using (var sr = new System.IO.StreamReader(view.Context.Assets.Open("editorScript.js")))
                editorScript = "javascript: " + sr.ReadToEnd();
            
            view.AddJavascriptInterface(new WebAppInterface(view.Context, (CustomWebView)view), "webViewInterface");
            view.EvaluateJavascript(editorScript,null);
            CommonConfig.Logger.Debug("JS ADDED");
        }
    }

    class WebAppInterface : Java.Lang.Object
    {
        Context context;
        CustomWebView webView;

        public WebAppInterface(Context context, CustomWebView webView)
        {
            this.context = context;
            this.webView = webView;
        }

        [Java.Interop.Export]
        [JavascriptInterface]
        public void OnTest()
        {
            CommonConfig.Logger.Debug("OSJDOJSIDOJSIDPJSPIDJSPDI");
        }

        [Java.Interop.Export]
        [JavascriptInterface]
        public void OnKeyPressed(int caretYcoord)
        {
            //See AbstractWebViewController.MoveViewToCaret
            CommonConfig.Logger.Debug("OnKeyPressed");
        }

        [Java.Interop.Export]
        [JavascriptInterface]
        public void OnEnterPressed(int caretYCoord)
        {
            CommonConfig.Logger.Debug("OnEnterPressed");
        }
    }
}