//
// Project: Mark5.Mobile.Droid
// File: ContentView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Webkit;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using Java.Interop;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class ContentView : ComposeDocumentView
    {
        readonly CustomWebView newContentWebView;
        readonly AppCompatButton showOldContentButton;
        readonly CustomWebView oldContentWebView;

        public SemaphoreSlim SetGetContentAsyncSemaphore = new SemaphoreSlim(1, 1);
        SemaphoreSlim getHtmlContentInterfaceSemaphore = new SemaphoreSlim(0, 1);
        string htmlContent;

        const string EditableContentClass = "content_c176f8ef-2579-4f1f-86c1-f289beaba2ae";
        const string TemplateElementClass = "template_75bb41fd-4984-43f5-b61d-3dbbe87bca21";

        const string DefaultEditContent = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">
                                        <html>
                                            <head>
                                            </head>
                                            <body style=""min-height: 200px;"">
                                                <div class=""" + EditableContentClass + @""" contenteditable=""true"" style=""width: 100%;""><br><br></div>
                                                <div class=""" + TemplateElementClass + @""" contenteditable=""true""></div>
                                            </body>
                                        </html>";


        bool oldContentLoaded;

        public ContentView(Context context)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            newContentWebView = new CustomWebView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            newContentWebView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            newContentWebView.AddJavascriptInterface(new GetHtmlContentInterface(this), "GetHtmlContentInterface");
            var customWebViewClient = new CustomWebViewClient();
            customWebViewClient.PageFinishedLoading += (sender, e) =>
            {
                SetGetContentAsyncSemaphore.Release();
            };
            newContentWebView.SetWebViewClient(customWebViewClient);
            newContentWebView.Settings.JavaScriptEnabled = true;
            newContentWebView.Settings.DomStorageEnabled = true;
            AddView(newContentWebView); //TODO check what kind of settings we need to enable

            showOldContentButton = new AppCompatButton(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            showOldContentButton.Text = "Show old content"; //TODO need to use resources and find a good text
            showOldContentButton.Visibility = ViewStates.Gone;
            showOldContentButton.Click += ShowOldContentButton_Click;
            AddView(showOldContentButton);

            oldContentWebView = new CustomWebView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            oldContentWebView.SetWebViewClient(new CustomWebViewClient());
            oldContentWebView.Settings.SetSupportZoom(true);
            oldContentWebView.Settings.BuiltInZoomControls = true;
            oldContentWebView.Settings.DisplayZoomControls = false;
            oldContentWebView.Settings.JavaScriptEnabled = false;
            oldContentWebView.VerticalScrollBarEnabled = false;
            oldContentWebView.HorizontalScrollBarEnabled = false;
            oldContentWebView.Visibility = ViewStates.Gone;
            AddView(oldContentWebView);
        }

        void ShowOldContentButton_Click(object sender, EventArgs e)
        {
            if (!oldContentLoaded)
            {
                if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
                {
                    oldContentWebView.LoadDataWithBaseURL(null, PreviousDocument.PlainTextBody, "text/plain", "UTF-8", null);
                }
                else if (!string.IsNullOrWhiteSpace(PreviousDocument.HtmlBody))
                {
                    oldContentWebView.LoadDataWithBaseURL(null, PreviousDocument.HtmlBody, "text/html", "UTF-8", null);
                }
                else
                {
                    oldContentWebView.LoadDataWithBaseURL(null, PreviousDocument.PlainTextBody, "text/plain", "UTF-8", null);
                }

                oldContentLoaded = true;
            }

            oldContentWebView.Visibility = ViewStates.Visible;
        }

        #region Public methods

        public override async Task RefreshView()
        {
            await SetHtmlContentAsync(DefaultEditContent);

            if (PreviousDocument != null)
            {
                showOldContentButton.Visibility = ViewStates.Visible;
            }
        }

        public override async Task UpdateDocument()
        {
            Document.HtmlBody = await GetHtmlContentAsync(true);
            DocumentPreview.Preview = GetPlainText(Document.HtmlBody);
        }

        public async Task InsertTemplate(Template template)
        {
            await SetWebContentPart(TemplateElementClass, template.ContentType, template.Content);
        }

        public async Task InsertLocalTemplate(string localTemplate)
        {
            await SetWebContentPart(TemplateElementClass, ContentType.PlainText, localTemplate);
        }

        #endregion

        #region Utility methods

        async Task SetHtmlContentAsync(string htmlString)
        {
            await SetGetContentAsyncSemaphore.WaitAsync();
            Post(() => newContentWebView.LoadDataWithBaseURL(null, htmlString, "text/html", "UTF-8", null));
        }

        async Task<string> GetHtmlContentAsync(bool processing)
        {
            try
            {
                await SetGetContentAsyncSemaphore.WaitAsync();
                Post(() => newContentWebView.LoadUrl(GetHtmlContentInterface.JavaScript));

                await getHtmlContentInterfaceSemaphore.WaitAsync();
            }
            finally
            {
                SetGetContentAsyncSemaphore.Release();
            }

            return processing ? await ProcessRetrievedContent(new HtmlParser(), htmlContent) : htmlContent;
        }

        async Task SetWebContentPart(string parentElementClass, ContentType contentType, string content)
        {
            var currentContent = await GetHtmlContentAsync(false);

            var htmlParser = new HtmlParser();
            var currentHtmlDocument = await htmlParser.ParseAsync(currentContent);
            var parentElements = currentHtmlDocument.QuerySelectorAll("div." + parentElementClass);

            if (parentElements.Length < 1)
            {
                CommonConfig.Logger.Error("Could not find div element with class: " + parentElementClass);
                return;
            }

            if (parentElements.Length > 1)
            {
                CommonConfig.Logger.Warning("Found more than one div element with class: " + parentElementClass);
            }

            var parentElement = parentElements[0];
            parentElement.Children.ForEach(c => c.Remove());

            var nodes = await ProcessInsertedContent(htmlParser, contentType, content);

            parentElement.Append(nodes.ToArray());

            var textWriter = new StringWriter();
            currentHtmlDocument.ToHtml(textWriter, HtmlMarkupFormatter.Instance);

            await SetHtmlContentAsync(textWriter.ToString());
        }

        static async Task<IHtmlCollection<IElement>> ProcessInsertedContent(HtmlParser htmlParser, ContentType contentToInsertType, string contentToInsert)
        {
            if (contentToInsertType == ContentType.Html)
            {
                var inlinedContentToInsert = await InlineStyles(contentToInsert);

                var htmlDocument = await htmlParser.ParseAsync(inlinedContentToInsert);

                return htmlDocument.Body.Children;
            }

            if (contentToInsertType == ContentType.PlainText)
            {
                var htmlDocument = await htmlParser.ParseAsync("<div>" + contentToInsert + "</div>");
                return htmlDocument.Body.Children;
            }

            throw new ArgumentException(string.Format("Unsupported contentType. [contentType={0}]", contentToInsertType));
        }

        static async Task<string> ProcessRetrievedContent(HtmlParser htmlParser, string content)
        {
            var currentHtmlDocument = await htmlParser.ParseAsync(content);

            var elementClasses = new[]
            {
                EditableContentClass,
                TemplateElementClass,
            };

            foreach (var elementClass in elementClasses)
            {
                var matchingElements = currentHtmlDocument.QuerySelectorAll("div." + elementClass);
                foreach (var matchingElement in matchingElements)
                {
                    matchingElement.Attributes.RemoveNamedItem("contenteditable");
                }
            }

            var processedWebContent = currentHtmlDocument.DocumentElement.OuterHtml;

            return await InlineStyles(processedWebContent);
        }

        static Task<string> InlineStyles(string content)
        {
            var tcs = new TaskCompletionSource<string>();

            //var inlineResult = PreMailer.Net.PreMailer.MoveCssInline(content, true, null, null, true, true);
            //if (inlineResult.Warnings != null && inlineResult.Warnings.Count > 0)
            //{
            //    if (Log.IsWarningEnabled())
            //    {
            //        Log.Warning("There were warnings when inlining CSS:\n" + string.Join("\n", inlineResult.Warnings));
            //    }
            //}

            tcs.SetResult(content); //TODO 
            return tcs.Task;
        }

        // Implementation based on http://stackoverflow.com/questions/5870438/get-plain-text-from-html-in-net
        static string GetPlainText(string htmlText)
        {
            return Regex.Replace(htmlText, @"<(.|\n)*?>", "");
        }

        #endregion

        #region Java script interfaces callbacks

        public void FinalizeGetHtmlContent(Java.Lang.String content)
        {
            htmlContent = (string)content;
            getHtmlContentInterfaceSemaphore.Release();
        }

        #endregion

        #region Javascript interfaces

        class GetHtmlContentInterface : Java.Lang.Object
        {
            public const string JavaScript = "javascript:window.GetHtmlContentInterface.getHtmlContent('<html>'+document.getElementsByTagName('html')[0].innerHTML+'</html>');";

            readonly ContentView view;

            public GetHtmlContentInterface(ContentView customWebViewRenderer)
            {
                view = customWebViewRenderer;
            }

            public GetHtmlContentInterface(IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
                : base(handle, transfer)
            {
            }

            [Export("getHtmlContent")]
            [JavascriptInterface]
            public void GetHtmlContent(Java.Lang.String htmlContent)
            {
                view.FinalizeGetHtmlContent(htmlContent);
            }
        }

        #endregion
    }
}
