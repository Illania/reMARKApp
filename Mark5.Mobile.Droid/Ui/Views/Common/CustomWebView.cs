using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Ui.Views.Common
{
    class CustomWebView : WebView
    {
        readonly Action<View, int> onInputAction;

        public CustomWebView(Context context, Action<View, int> onInputAction)
            : base(context)
        {
            this.onInputAction = onInputAction;
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

        public void OnInput(int caretYcoord)
        {
            onInputAction?.Invoke(this, caretYcoord);
        }

        public void SetBold()
        {
            var js = "document.execCommand( 'bold',false,null);";
            EvaluateJavascript(js, null);
        }

        public void SetItalic()
        {
            var js = "document.execCommand( 'italic',false,null);";
            EvaluateJavascript(js, null);
        }

        public void SetUnderline()
        {
            var js = "document.execCommand( 'underline',false,null);";
            EvaluateJavascript(js, null);
        }
    }

    class CustomWebViewClient : WebViewClient
    {
        public event EventHandler PageFinishedLoading = delegate { };

        [Obsolete]
        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            try
            {
                view.Context.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
            }
            catch(Exception ex)
            {
                CommonConfig.Logger.Warning(ex.Message);
            }
            return true;
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            PageFinishedLoading(this, EventArgs.Empty);

            string editorScript;
            using (var sr = new System.IO.StreamReader(view.Context.Assets.Open("editorScript.js")))
                editorScript = "javascript: " + sr.ReadToEnd();

            view.AddJavascriptInterface(new WebAppInterface((CustomWebView)view), "Android");
            view.LoadUrl(editorScript);
        }
    }

    class WebAppInterface : Java.Lang.Object
    {
        readonly CustomWebView webView;

        public WebAppInterface(CustomWebView webView)
        {
            this.webView = webView;
        }

        [Java.Interop.Export]
        [JavascriptInterface]
        public void OnInput(int caretYcoord) => webView.OnInput(caretYcoord);

    }
}

