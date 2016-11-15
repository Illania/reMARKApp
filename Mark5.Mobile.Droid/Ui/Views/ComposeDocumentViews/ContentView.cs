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
using System.Text;
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
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class ContentView : ComposeDocumentView
    {
        readonly CustomWebView newContentWebView;
        readonly AppCompatButton showOldContentButton;
        readonly CustomWebView oldContentWebView;

        public SemaphoreSlim SetGetContentAsyncSemaphore = new SemaphoreSlim(1, 1);
        SemaphoreSlim getHtmlContentInterfaceSemaphore = new SemaphoreSlim(0, 1);

        string newContent;
        string oldContent;

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
            Orientation = Vertical;
            var layoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, 0);
            layoutParams.Weight = 1;
            LayoutParameters = layoutParams;
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
            showOldContentButton.SetText(Resource.String.show_previous_message);
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

        async void ShowOldContentButton_Click(object sender, EventArgs e)
        {
            if (oldContentWebView.Visibility == ViewStates.Gone)
            {
                await LoadOldContent();

                showOldContentButton.SetText(Resource.String.hide_previous_message);
                oldContentWebView.Visibility = ViewStates.Visible;
            }
            else
            {
                oldContentWebView.Visibility = ViewStates.Gone;
                showOldContentButton.SetText(Resource.String.show_previous_message);
            }
        }

        async Task LoadOldContent()
        {
            if (!oldContentLoaded && PreviousDocument != null)
            {
                if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
                {
                    oldContent = await GetBodyWithHeader(PreviousDocument.PlainTextBody, ContentType.PlainText);
                    oldContentWebView.LoadDataWithBaseURL(null, oldContent, "text/html", "UTF-8", null);
                }
                else if (!string.IsNullOrWhiteSpace(PreviousDocument.HtmlBody))
                {
                    oldContent = await GetBodyWithHeader(PreviousDocument.HtmlBody, ContentType.Html);
                    oldContentWebView.LoadDataWithBaseURL(null, oldContent, "text/html", "UTF-8", null);
                }
                else
                {
                    oldContent = await GetBodyWithHeader(PreviousDocument.PlainTextBody, ContentType.PlainText);
                    oldContentWebView.LoadDataWithBaseURL(null, oldContent, "text/html", "UTF-8", null); //TODO need to test
                }

                oldContentLoaded = true;
            }
        }

        async Task<string> GetBodyWithHeader(string content, ContentType contentType)
        {
            if (contentType == ContentType.Html)
            {
                var htmlHeader = GetHtmlHeader();

                var htmlParser = new HtmlParser();
                var htmlDocument = await htmlParser.ParseAsync(content);
                var body = htmlDocument.Body;
                var parsedHeader = await htmlParser.ParseAsync(htmlHeader);
                body.InsertBefore(parsedHeader.Body, body.FirstChild);

                var textWriter = new StringWriter();
                htmlDocument.ToHtml(textWriter, HtmlMarkupFormatter.Instance);

                return textWriter.ToString();
            }

            if (contentType == ContentType.PlainText)
            { //TODO need to test this
                var htmlHeader = GetHtmlHeader();

                var htmlParser = new HtmlParser();
                var parsedHeader = await htmlParser.ParseAsync(htmlHeader);

                var contentHtml = parsedHeader.CreateElement("p");
                contentHtml.TextContent = content;

                parsedHeader.Body.Append(contentHtml);

                var textWriter = new StringWriter();
                parsedHeader.ToHtml(textWriter, HtmlMarkupFormatter.Instance);

                return textWriter.ToString();
            }

            return "";
        }

        string GetHtmlHeader()
        {
            var processedDateReceivedTimestamp = PreviousDocumentPreview.DateReceivedTimestamp
                                                                        .ConvertTimestampMillisecondsToDateTime()
                                                                        .ConvertUtcToServerTime()
                                                                        .ConvertDateTimeToTimestampMilliseconds();
            var date = processedDateReceivedTimestamp.FormatServerTimestampAsTimeAndDateString(Context);

            var header = new StringBuilder();
            header.Append("<br/><hr/>");
            header.Append(string.Format("<b>From</b>: {0}", GetAddressTextFromPreviousDocument(DocumentAddressType.From))).Append("</br>");
            header.Append(string.Format("<b>Date</b>: {0}", date)).Append("</br>");
            header.Append(string.Format("<b>To</b>: {0}", GetAddressTextFromPreviousDocument(DocumentAddressType.To))).Append("</br>");
            var ccText = GetAddressTextFromPreviousDocument(DocumentAddressType.Cc);
            if (!string.IsNullOrWhiteSpace(ccText))
            {
                header.Append(string.Format("<b>Cc</b>: {0}", ccText)).Append("</br>");
            }
            header.Append(string.Format("<b>Subject</b>: {0}", PreviousDocumentPreview.Subject)).Append("</br>");
            header.Append("<br/><br/>");

            return header.ToString();
        }

        #region Public methods

        public override async Task RefreshView()
        {
            await SetHtmlContentAsync(DefaultEditContent);

            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                if (!string.IsNullOrWhiteSpace(PreviousDocument.HtmlBody))
                {
                    await SetWebContentPart(EditableContentClass, ContentType.Html, PreviousDocument.HtmlBody);
                }
                else
                {
                    await SetWebContentPart(EditableContentClass, ContentType.PlainText, PreviousDocument.PlainTextBody);
                }
            }
            else
            {
                if (PreviousDocument != null)
                {
                    showOldContentButton.Visibility = ViewStates.Visible;
                }
            }
        }

        public override async Task UpdateDocument()
        {
            Document.HtmlBody = await RetrieveCombinedText(); //TODO need to test this
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

        async Task<string> RetrieveCombinedText()
        {
            var newContentString = await GetHtmlContentAsync(true);

            await LoadOldContent();

            if (!string.IsNullOrEmpty(oldContent))
            {
                var htmlParser = new HtmlParser();
                var newContentParsed = await htmlParser.ParseAsync(newContentString);
                var oldContentParsed = await htmlParser.ParseAsync(oldContent);
                var oldContentInDiv = newContentParsed.CreateElement("div");
                oldContentInDiv.InnerHtml = oldContentParsed.Body.InnerHtml;
                newContentParsed.Body.Append(oldContentInDiv);
                var textWriter = new StringWriter();
                newContentParsed.ToHtml(textWriter, HtmlMarkupFormatter.Instance);
                return textWriter.ToString();
            }

            return newContentString;
        }

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

            return processing ? await ProcessRetrievedContent(new HtmlParser(), newContent) : newContent;
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

        string GetAddressTextFromPreviousDocument(DocumentAddressType addressType)
        {
            var sb = new StringBuilder();
            var addresses = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == addressType).ToList();
            for (int i = 0; i < addresses.Count; i++)
            {
                var hasName = !string.IsNullOrWhiteSpace(addresses[i].Name);
                if (hasName)
                {
                    sb.Append(addresses[i].Name).Append(" &lt;");
                }
                sb.Append(addresses[i].Address);
                if (hasName)
                {
                    sb.Append("&gt;");
                }
                if (i < addresses.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }

        #endregion

        #region Java script interfaces callbacks

        public void FinalizeGetHtmlContent(Java.Lang.String content)
        {
            newContent = (string)content;
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
