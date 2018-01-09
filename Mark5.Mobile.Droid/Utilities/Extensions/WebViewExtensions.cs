using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Webkit;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Droid.Utilities;
using Newtonsoft.Json;

namespace Mark5.Mobile.IOS.Utilities.Extensions
{
    public static class WebViewExtensions
    {

        public static async Task LoadHtml(this WebView webView, Context ctx, string html, HtmlProcessingConfiguration config)
        {
            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info("Starting processing of the document...");

            var sw = Stopwatch.StartNew();

            html = await HtmlUtilities.ProcessHtml(ctx, html, config);

            sw.Stop();

            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info($"Processing document took: {sw.ElapsedMilliseconds}ms.");

            webView.StopLoading();
            webView.LoadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
        }

        public static async Task LoadPlainText(this WebView webView, Context ctx, string html, PlainTextProcessingConfiguration config)
        {
            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info("Starting processing of the document...");

            var sw = Stopwatch.StartNew();

            html = await HtmlUtilities.ProcessPlainText(ctx, html, config);

            sw.Stop();

            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info($"Processing document took: {sw.ElapsedMilliseconds}ms.");

            webView.StopLoading();
            webView.LoadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
        }

        public static void LoadEditor(this WebView webView, Context ctx)
        {
            string html;
            using (var sr = new StreamReader(ctx.Assets.Open("editor.html")))
                html = sr.ReadToEnd();

            webView.StopLoading();
            webView.LoadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
        }

        public static void LoadNoContentString(this WebView webView, Context ctx)
        {
            string html;
            using (var sr = new StreamReader(ctx.Assets.Open("empty.html")))
                html = sr.ReadToEnd();

            webView.StopLoading();
            webView.LoadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
        }

        public static void LoadEmpty(this WebView webView)
        {
            webView.StopLoading();
            webView.LoadDataWithBaseURL(null, "", "text/html", "UTF-8", null);
        }

        public static Task<string> EvaluateJavaScriptAsync(this WebView webView, string js)
        {
            var tcs = new TaskCompletionSource<string>();
            webView.EvaluateJavascript(js, new ValueCallBack(tcs));
            return tcs.Task;
        }

        class ValueCallBack : Java.Lang.Object, IValueCallback
        {
            readonly TaskCompletionSource<string> tcs;

            public ValueCallBack(TaskCompletionSource<string> tcs)
            {
                this.tcs = tcs;
            }

            public void OnReceiveValue(Java.Lang.Object value)
            {
                try
                {
                    var str = (Java.Lang.String)value;
                    var result = JsonConvert.DeserializeObject<string>(str.ToString());
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
        }
    }
}