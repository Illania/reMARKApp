using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;

namespace Mark5.Mobile.Droid.Ui.Views.Common
{
    class CustomWebView : WebView
    {
        readonly Action<View, int> onInputAction;
        readonly Action onStartActionMode;
        readonly Action onDestroyActionMode;

        public CustomWebView(Context context, Action<View, int> onInputAction, Action onStartActionMode, Action onDestroyActionMode)
            : base(context)
        {
            this.onInputAction = onInputAction;
            this.onStartActionMode = onStartActionMode;
            this.onDestroyActionMode = onDestroyActionMode;
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

        //We need two methods because they are used for different versions of Android
        public override ActionMode StartActionMode(ActionMode.ICallback callback, [GeneratedEnum] ActionModeType type)
        {
            var myCallback = new CustomActionModeCallback(onDestroyActionMode);
            onStartActionMode?.Invoke();
            return base.StartActionMode(myCallback, type);
        }

        public override ActionMode StartActionMode(ActionMode.ICallback callback)
        {
            var myCallback = new CustomActionModeCallback(onDestroyActionMode);
            onStartActionMode?.Invoke();
            return base.StartActionMode(myCallback);
        }

        private class CustomActionModeCallback : Java.Lang.Object, ActionMode.ICallback
        {
            private readonly Action onDestroyActionMode;

            public CustomActionModeCallback(Action onDestroyActionMode)
            {
                this.onDestroyActionMode = onDestroyActionMode;
            }

            public bool OnActionItemClicked(ActionMode mode, IMenuItem item) => false;

            public bool OnCreateActionMode(ActionMode mode, IMenu menu) => true;

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu) => false;

            public void OnDestroyActionMode(ActionMode mode) => onDestroyActionMode?.Invoke();

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

