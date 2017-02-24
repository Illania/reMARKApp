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
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
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
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class ContentView : ComposeDocumentView
    {
        readonly CustomWebView newContentWebView;
        readonly AppCompatButton showOldContentButton;
        readonly CustomWebView oldContentWebView;
        readonly Context context;

        public SemaphoreSlim SetGetContentAsyncSemaphore = new SemaphoreSlim(1, 1);
        SemaphoreSlim getHtmlContentInterfaceSemaphore = new SemaphoreSlim(0, 1);

        string newContent;
        string oldContent;

        const string EditableContentClass = "content_c176f8ef-2579-4f1f-86c1-f289beaba2ae";

        const string DefaultEditContent = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">
                                            <html>
                                                <head>
                                                </head>
                                                <body style=""min-height: 500px;"">
                                                    <div class=""" + EditableContentClass + @""" contenteditable=""true"" style=""width: 100%; outline: 0px solid transparent""><br><br></div>
                                                </body>
                                            </html>";

        bool oldContentLoaded;

        public ContentView(Context context)
            : base(context)
        {
            this.context = context;
            Orientation = Vertical;
            var layoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, 0);
            layoutParams.Weight = 1;
            LayoutParameters = layoutParams;
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            newContentWebView = new CustomWebView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            newContentWebView.SetBackgroundColor(Color.Transparent);
            newContentWebView.AddJavascriptInterface(new GetHtmlContentInterface(this), "GetHtmlContentInterface");
            var customWebViewClient = new CustomWebViewClient();
            customWebViewClient.PageFinishedLoading += (sender, e) =>
            {
                SetGetContentAsyncSemaphore.Release();
            };
            newContentWebView.SetWebViewClient(customWebViewClient);
            newContentWebView.Settings.JavaScriptEnabled = true;
            newContentWebView.Settings.DomStorageEnabled = true;
            AddView(newContentWebView);

            showOldContentButton = new AppCompatButton(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            showOldContentButton.SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();
            showOldContentButton.SetTextAppearanceCompat(context, Resource.Style.fontSmallBold);
            showOldContentButton.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.brown)));
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
                showOldContentButton.Enabled = false;
                showOldContentButton.SetText(Resource.String.showing_previous_message);

                await Task.Delay(100); // Let the button animation run
                await LoadOldContent();

                showOldContentButton.SetText(Resource.String.hide_previous_message);
                showOldContentButton.Enabled = true;
                oldContentWebView.Visibility = ViewStates.Visible;
            }
            else
            {
                oldContentWebView.Visibility = ViewStates.Gone;
                showOldContentButton.SetText(Resource.String.show_previous_message);
            }
        }

        #region Public methods

        public override async Task RefreshView()
        {
            if (State != null)
            {
                await RestoreState();
                State = null;
                return;
            }

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
            Document.HtmlBody = await RetrieveCombinedText();
            DocumentPreview.Preview = await GetPreview(Document.HtmlBody);
        }

        public async Task InsertTemplate(Template template)
        {
            await SetWebContentPart(EditableContentClass, template.ContentType, template.Content);
        }

        public async Task InsertLocalTemplate(string localTemplate)
        {
            await SetWebContentPart(EditableContentClass, ContentType.PlainText, localTemplate);
        }

        #endregion

        #region Utility methods

        async Task LoadOldContent()
        {
            if (!oldContentLoaded && CreationModeFlag != DocumentCreationModeFlag.Edit && PreviousDocument != null)
            {
                if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
                {
                    oldContent = await GetBodyWithHeader(PreviousDocument.PlainTextBody, ContentType.PlainText);
                }
                else if (!string.IsNullOrWhiteSpace(PreviousDocument.HtmlBody))
                {
                    oldContent = await GetBodyWithHeader(PreviousDocument.HtmlBody, ContentType.Html);
                }
                else
                {
                    oldContent = await GetBodyWithHeader(PreviousDocument.PlainTextBody, ContentType.PlainText);
                }

                ((Activity)context).RunOnUiThread(() => oldContentWebView.LoadDataWithBaseURL(null, oldContent, "text/html", "UTF-8", null));
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
            {
                var htmlHeader = GetHtmlHeader();

                var htmlParser = new HtmlParser();
                var parsedHeader = await htmlParser.ParseAsync(htmlHeader);

                var contentHtml = parsedHeader.CreateElement("pre");
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
            var fromText = PreviousDocumentPreview.Direction == DocumentDirection.Incoming ? GetAddressTextFromPreviousDocument(DocumentAddressType.From) : GetLinesTextFromPreviousDocument();
            if (!string.IsNullOrWhiteSpace(fromText))
            {
                header.Append($"<b>From</b>: {fromText}").Append("</br>");
            }
            header.Append($"<b>Date</b>: {date}").Append("</br>");
            header.Append($"<b>To</b>: {GetAddressTextFromPreviousDocument(DocumentAddressType.To)}").Append("</br>");
            var ccText = GetAddressTextFromPreviousDocument(DocumentAddressType.Cc);
            if (!string.IsNullOrWhiteSpace(ccText))
            {
                header.Append($"<b>Cc</b>: {ccText}").Append("</br>");
            }
            header.Append($"<b>Subject</b>: {PreviousDocumentPreview.Subject}").Append("</br>");
            header.Append("<br/><br/>");

            return header.ToString();
        }


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
            ((Activity)context).RunOnUiThread(() => newContentWebView.LoadDataWithBaseURL(null, htmlString, "text/html", "UTF-8", null));
        }

        async Task<string> GetHtmlContentAsync(bool processing)
        {
            try
            {
                await SetGetContentAsyncSemaphore.WaitAsync();
                ((Activity)context).RunOnUiThread(() => newContentWebView.LoadUrl(GetHtmlContentInterface.JavaScript));

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
            parentElement.TextContent = string.Empty;

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
                var htmlDocument = await htmlParser.ParseAsync("<div><pre>" + contentToInsert + "</pre></div>");
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

            var inlineResult = PreMailer.Net.PreMailer.MoveCssInline(content, true, null, null, true, true);
            if (inlineResult.Warnings != null && inlineResult.Warnings.Count > 0)
            {
                CommonConfig.Logger.Warning("There were warnings when inlining CSS:\n" + string.Join("\n", inlineResult.Warnings));
            }

            tcs.SetResult(inlineResult.Html);
            return tcs.Task;
        }

        static async Task<string> GetPreview(string htmlText)
        {
            var htmlParser = new HtmlParser();
            var newContentParsed = await htmlParser.ParseAsync(htmlText);
            var textContent = newContentParsed.Body.TextContent;
            return textContent.SafeSubstring(0, 300).TrimStart();
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

        string GetLinesTextFromPreviousDocument()
        {
            return string.Join(", ", PreviousDocument.Lines.Select(l => l.FromAddress));
        }

        #endregion

        #region State related

        async Task RestoreState()
        {
            var contentViewState = State as ContentViewState;
            oldContent = contentViewState.OldContent;
            oldContentLoaded = contentViewState.OldContentLoaded;
            oldContentWebView.Visibility = contentViewState.OldContentWebViewVisibility;
            showOldContentButton.Visibility = contentViewState.ShowOldContentButtonVisibility;
            showOldContentButton.Text = contentViewState.ShowOldContentButtonText;

            await SetHtmlContentAsync(contentViewState.NewContent);

            if (oldContentLoaded)
            {
                ((Activity)context).RunOnUiThread(() => oldContentWebView.LoadDataWithBaseURL(null, oldContent, "text/html", "UTF-8", null));
            }
        }

        public override IComposeDocumentViewState ReturnState()
        {
            var newContentString = AsyncHelpers.RunSync(() => { return GetHtmlContentAsync(false); });
            return new ContentViewState
            {
                NewContent = newContentString,
                OldContent = oldContent,
                OldContentLoaded = oldContentLoaded,
                OldContentWebViewVisibility = oldContentWebView.Visibility,
                ShowOldContentButtonVisibility = showOldContentButton.Visibility,
                ShowOldContentButtonText = showOldContentButton.Text,
            };
        }

        class ContentViewState : IComposeDocumentViewState
        {
            public string NewContent { get; set; }
            public string OldContent { get; set; }
            public bool OldContentLoaded { get; set; }
            public ViewStates OldContentWebViewVisibility { get; set; }
            public ViewStates ShowOldContentButtonVisibility { get; set; }
            public string ShowOldContentButtonText { get; set; }
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
