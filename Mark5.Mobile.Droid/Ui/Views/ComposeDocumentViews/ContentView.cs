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
using AngleSharp.Dom.Html;
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

        bool oldContentShown;

        public SemaphoreSlim NewSetGetContentAsyncSemaphore = new SemaphoreSlim(1, 1);
        SemaphoreSlim newGetHtmlContentInterfaceSemaphore = new SemaphoreSlim(0, 1);

        public SemaphoreSlim OldSetGetContentAsyncSemaphore = new SemaphoreSlim(1, 1);
        SemaphoreSlim oldGetHtmlContentInterfaceSemaphore = new SemaphoreSlim(0, 1);

        string newContent;
        string oldContent;

        const string NewEditableContentClass = "new_content_c176f8ef-2579-4f1f-86c1-f289beaba2ae";
        const string OldEditableContentClass = "old_content_cc4ee2cb-e18c-423a-adb2-d106a29dcbc3";

        const string DefaultEditContent = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">
                                            <html>
                                                <head>
                                                </head>
                                                <body style=""min-height: 500px;"">
                                                    <div class=""" +
                                          NewEditableContentClass +
                                          @""" contenteditable=""true"" style=""font-family: sans-serif; width: 100%; outline: 0px solid transparent""><br><br></div>
                                                </body>
                                            </html>";

        bool oldContentLoaded;

        public ContentView(Context context)
            : base(context)
        {
            this.context = context;
            Orientation = Vertical;
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, 0)
            {
                Weight = 1
            };
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            newContentWebView = new CustomWebView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            newContentWebView.SetBackgroundColor(Color.Transparent);
            newContentWebView.AddJavascriptInterface(new GetHtmlContentInterface(this), "GetHtmlContentInterface");
            var customWebViewClient = new CustomWebViewClient();
            customWebViewClient.PageFinishedLoading += (sender, e) => { NewSetGetContentAsyncSemaphore.Release(); };
            newContentWebView.SetWebViewClient(customWebViewClient);
            newContentWebView.Settings.JavaScriptEnabled = true;
            newContentWebView.Settings.DomStorageEnabled = true;
            AddView(newContentWebView);

            showOldContentButton = new AppCompatButton(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            var typedArray = Context.ObtainStyledAttributes(new int[]
            {
                Resource.Attribute.selectableItemBackground
            });
            showOldContentButton.SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();
            showOldContentButton.SetTextAppearanceCompat(context, Resource.Style.fontSmallBold);
            showOldContentButton.SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkgray)));
            showOldContentButton.SetText(Resource.String.show_previous_message);
            showOldContentButton.Visibility = ViewStates.Gone;
            showOldContentButton.Click += ShowOldContentButton_Click;
            AddView(showOldContentButton);

            oldContentWebView = new CustomWebView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            var oldContentWebViewClient = new CustomWebViewClient();
            oldContentWebViewClient.PageFinishedLoading += (sender, e) => { OldSetGetContentAsyncSemaphore.Release(); };
            oldContentWebView.SetWebViewClient(oldContentWebViewClient);
            oldContentWebView.AddJavascriptInterface(new GetHtmlContentInterface(this), "GetHtmlContentInterface");
            oldContentWebView.Settings.JavaScriptEnabled = true;
            oldContentWebView.Visibility = ViewStates.Gone;
            AddView(oldContentWebView);
        }

        void ShowOldContentButton_Click(object sender, EventArgs e)
        {
            if (!oldContentShown)
            {
                oldContentShown = true;
                CommonConfig.UsageAnalytics.LogEvent(new ComposeShowPreviousEmailEvent());
            }

            if (oldContentWebView.Visibility == ViewStates.Gone)
            {
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

            await SetNewHtmlContentAsync(DefaultEditContent);
            if (RestoreWorkingCopy)
            {
                await SetWebContentPart(NewEditableContentClass, ContentType.Html, Document.HtmlBody);
                return;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit ||
                (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepTextAndAttachments))
            {
                if (!string.IsNullOrWhiteSpace(PreviousDocument.HtmlBody))
                    await SetWebContentPart(NewEditableContentClass, ContentType.Html, PreviousDocument.HtmlBody);
                else
                    await SetWebContentPart(NewEditableContentClass, ContentType.PlainText, PreviousDocument.PlainTextBody);
            }
            else
            {
                showOldContentButton.Visibility = DocumentCreationModeFlag == DocumentCreationModeFlag.New ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.Edit ||
                    PreviousDocument == null ? ViewStates.Gone : ViewStates.Visible;

                await LoadOldContent();
            }
        }

        public override async Task UpdateDocument()
        {
            (Document.HtmlBody, DocumentPreview.Preview) = await RetrieveCombinedText();
        }

        public async Task InsertTemplate(Template template)
        {
            await SetWebContentPart(NewEditableContentClass, template.ContentType, "<br>" + template.Content);
        }

        public async Task InsertLocalTemplate(string localTemplate)
        {
            await SetWebContentPart(NewEditableContentClass, ContentType.PlainText, "\n" + localTemplate);
        }

        #endregion

        #region Utility methods

        async Task LoadOldContent()
        {
            if (!oldContentLoaded && DocumentCreationModeFlag != DocumentCreationModeFlag.Edit && DocumentCreationModeFlag != DocumentCreationModeFlag.New && PreviousDocument != null)
            {
                string oldContentString = null;

                if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
                    oldContentString = await GetBodyWithHeader(PreviousDocument.PlainTextBody, ContentType.PlainText);
                else if (!string.IsNullOrWhiteSpace(PreviousDocument.HtmlBody))
                    oldContentString = await GetBodyWithHeader(PreviousDocument.HtmlBody, ContentType.Html);
                else
                    oldContentString = await GetBodyWithHeader(PreviousDocument.PlainTextBody, ContentType.PlainText);

                await SetOldHtmlContentAsync(oldContentString);
                oldContentLoaded = true;
            }
        }

        async Task<string> GetBodyWithHeader(string content, ContentType contentType)
        {
            return await Task.Run(async () =>
           {
               if (contentType == ContentType.Html)
               {
                   var htmlHeader = GetHtmlHeader();

                   var htmlParser = new HtmlParser();
                   var htmlDocument = await htmlParser.ParseAsync(content);
                   var body = htmlDocument.Body;
                   var parsedHeader = await htmlParser.ParseAsync(htmlHeader);
                   body.InsertBefore(parsedHeader.Body, body.FirstChild);

                   var ce = htmlDocument.CreateElement("div");
                   ce.ClassName = OldEditableContentClass;
                   ce.Id = "editable-one";
                   ce.SetAttribute("contentEditable", "true");
                   ce.SetAttribute("style", "outline: 0px solid transparent");

                   ce.InnerHtml = body.InnerHtml;
                   body.InnerHtml = ce.OuterHtml;

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

                   var ce = parsedHeader.CreateElement("div");
                   ce.ClassName = OldEditableContentClass;
                   ce.Id = "editable-one";
                   ce.SetAttribute("contentEditable", "true");
                   ce.SetAttribute("style", "outline: 0px solid transparent");

                   ce.InnerHtml = parsedHeader.Body.InnerHtml;
                   parsedHeader.Body.InnerHtml = ce.OuterHtml;

                   var textWriter = new StringWriter();
                   parsedHeader.ToHtml(textWriter, HtmlMarkupFormatter.Instance);

                   return textWriter.ToString();
               }

               return "";
           });
        }

        string GetHtmlHeader()
        {
            var processedDateReceivedTimestamp = PreviousDocumentPreview.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();
            var date = processedDateReceivedTimestamp.FormatUserTimestampAsTimeAndDateString(Context);

            var header = new StringBuilder();
            header.Append("<br/><hr/>");
            var fromText = PreviousDocumentPreview.Direction == DocumentDirection.Incoming ? GetAddressTextFromPreviousDocument(DocumentAddressType.From) : GetLinesTextFromPreviousDocument();
            if (!string.IsNullOrWhiteSpace(fromText))
                header.Append($"<b>From</b>: {fromText}").Append("</br>");
            header.Append($"<b>Date</b>: {date}").Append("</br>");
            header.Append($"<b>To</b>: {GetAddressTextFromPreviousDocument(DocumentAddressType.To)}").Append("</br>");
            var ccText = GetAddressTextFromPreviousDocument(DocumentAddressType.Cc);
            if (!string.IsNullOrWhiteSpace(ccText))
                header.Append($"<b>Cc</b>: {ccText}").Append("</br>");
            header.Append($"<b>Subject</b>: {PreviousDocumentPreview.Subject}").Append("</br>");
            header.Append("<br/><br/>");

            return header.ToString();
        }

        async Task<(string content, string preview)> RetrieveCombinedText()
        {
            var htmlParser = new HtmlParser();

            var newContentString = await AsyncHelpers.RunOnUiThreadAsync((Activity)Context, () => GetNewHtmlContentAsync());
            var newContentParsed = await htmlParser.ParseAsync(newContentString);
            var newContentProcessedString = await ProcessRetrievedContent(newContentParsed, NewEditableContentClass);

            if (oldContentLoaded)
            {
                var oldContentString = await AsyncHelpers.RunOnUiThreadAsync((Activity)Context, () => GetOldHtmlContentAsync());
                var oldContentParsed = await htmlParser.ParseAsync(oldContentString);
                var oldContentProcessed = await ProcessRetrievedContent(oldContentParsed, OldEditableContentClass);
                var oldContentProcessedParsed = await htmlParser.ParseAsync(oldContentProcessed);

                var newContentProcessedParsed = await htmlParser.ParseAsync(newContentProcessedString);

                var oldContentInDiv = newContentProcessedParsed.CreateElement("div");
                oldContentInDiv.InnerHtml = oldContentProcessedParsed.Body.InnerHtml;
                newContentProcessedParsed.Body.Append(oldContentInDiv);
                var textWriter = new StringWriter();
                newContentProcessedParsed.ToHtml(textWriter, HtmlMarkupFormatter.Instance);
                var content = textWriter.ToString();
                var preview = GetPreview(newContentProcessedParsed);
                return (content, preview);
            }

            return (newContentProcessedString, GetPreview(newContentParsed));
        }

        #region Get/Set contents

        async Task SetNewHtmlContentAsync(string htmlString)
        {
            await NewSetGetContentAsyncSemaphore.WaitAsync();
            ((Activity)context).RunOnUiThread(() => newContentWebView.LoadDataWithBaseURL(null, htmlString, "text/html", "UTF-8", null));
        }

        async Task<string> GetNewHtmlContentAsync()
        {
            try
            {
                await NewSetGetContentAsyncSemaphore.WaitAsync();
                ((Activity)context).RunOnUiThread(() => newContentWebView.LoadUrl(GetHtmlContentInterface.NewContentJavascript));

                await newGetHtmlContentInterfaceSemaphore.WaitAsync();
            }
            finally
            {
                NewSetGetContentAsyncSemaphore.Release();
            }

            return newContent;
        }

        async Task SetOldHtmlContentAsync(string htmlString)
        {
            await OldSetGetContentAsyncSemaphore.WaitAsync();
            ((Activity)context).RunOnUiThread(() => oldContentWebView.LoadDataWithBaseURL(null, htmlString, "text/html", "UTF-8", null));
        }

        async Task<string> GetOldHtmlContentAsync()
        {
            try
            {
                await OldSetGetContentAsyncSemaphore.WaitAsync();
                ((Activity)context).RunOnUiThread(() => oldContentWebView.LoadUrl(GetHtmlContentInterface.OldContentJavascript));

                await oldGetHtmlContentInterfaceSemaphore.WaitAsync();
            }
            finally
            {
                OldSetGetContentAsyncSemaphore.Release();
            }

            return oldContent;
        }

        #endregion

        async Task SetWebContentPart(string parentElementClass, ContentType contentType, string content)
        {
            var currentContent = await GetNewHtmlContentAsync();

            var htmlParser = new HtmlParser();
            var currentHtmlDocument = await htmlParser.ParseAsync(currentContent);

            var parentElements = currentHtmlDocument.QuerySelectorAll("div." + parentElementClass);

            if (parentElements.Length < 1)
            {
                CommonConfig.Logger.Error("Could not find div element with class: " + parentElementClass);
                return;
            }

            if (parentElements.Length > 1)
                CommonConfig.Logger.Warning("Found more than one div element with class: " + parentElementClass);

            var parentElement = parentElements[0];
            parentElement.Children.ForEach(c => c.Remove());
            parentElement.TextContent = string.Empty;

            var precessedContent = await ProcessInsertedContent(htmlParser, contentType, content);

            parentElement.InnerHtml = precessedContent;

            var textWriter = new StringWriter();
            currentHtmlDocument.ToHtml(textWriter, HtmlMarkupFormatter.Instance);

            await SetNewHtmlContentAsync(textWriter.ToString());
        }

        static async Task<string> ProcessInsertedContent(HtmlParser htmlParser, ContentType contentToInsertType, string contentToInsert)
        {
            if (contentToInsertType == ContentType.Html)
            {
                var inlinedContentToInsert = await InlineStyles(contentToInsert);

                var htmlDocument = await htmlParser.ParseAsync(inlinedContentToInsert);

                return htmlDocument.Body.InnerHtml;
            }

            if (contentToInsertType == ContentType.PlainText)
            {
                var htmlDocument = await htmlParser.ParseAsync("<div><pre>" + contentToInsert + "</pre></div>");
                return htmlDocument.Body.InnerHtml;
            }

            throw new ArgumentException(string.Format("Unsupported contentType. [contentType={0}]", contentToInsertType));
        }

        static async Task<string> ProcessRetrievedContent(IHtmlDocument currentHtmlDocument, string elementClass)
        {
            var matchingElements = currentHtmlDocument.QuerySelectorAll("div." + elementClass);
            foreach (var matchingElement in matchingElements)
                matchingElement.Attributes.RemoveNamedItem("contenteditable");

            var processedWebContent = currentHtmlDocument.DocumentElement.OuterHtml;

            return await InlineStyles(processedWebContent);
        }

        static Task<string> InlineStyles(string content)
        {
            var tcs = new TaskCompletionSource<string>();

            var inlineResult = PreMailer.Net.PreMailer.MoveCssInline(content, true, null, null, true, true);
            if (inlineResult.Warnings != null && inlineResult.Warnings.Count > 0)
                CommonConfig.Logger.Warning("There were warnings when inlining CSS:\n" + string.Join("\n", inlineResult.Warnings));

            tcs.SetResult(inlineResult.Html);
            return tcs.Task;
        }

        static string GetPreview(IHtmlDocument contentParsed)
        {
            var textContent = contentParsed.Body.TextContent;
            return textContent.SafeSubstring(0, 300).TrimStart();
        }

        string GetAddressTextFromPreviousDocument(DocumentAddressType addressType)
        {
            var sb = new StringBuilder();
            var addresses = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == addressType).ToList();
            for (var i = 0; i < addresses.Count; i++)
            {
                var hasName = !string.IsNullOrWhiteSpace(addresses[i].Name);
                if (hasName)
                    sb.Append(addresses[i].Name).Append(" &lt;");
                sb.Append(addresses[i].Address);
                if (hasName)
                    sb.Append("&gt;");
                if (i < addresses.Count - 1)
                    sb.Append(", ");
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
            var contentViewState = (ContentViewState)State;
            oldContentLoaded = contentViewState.OldContentLoaded;
            oldContentWebView.Visibility = contentViewState.OldContentWebViewVisibility;
            showOldContentButton.Visibility = contentViewState.ShowOldContentButtonVisibility;
            showOldContentButton.Text = contentViewState.ShowOldContentButtonText;

            await SetNewHtmlContentAsync(contentViewState.NewContent);

            if (oldContentLoaded)
                await SetOldHtmlContentAsync(contentViewState.OldContent);
        }

        public override IComposeDocumentViewState GetState()
        {
            var newContentString = AsyncHelpers.RunSync(() => { return GetNewHtmlContentAsync(); });
            var oldContentString = AsyncHelpers.RunSync(() => { return GetOldHtmlContentAsync(); });

            return new ContentViewState
            {
                NewContent = newContentString,
                OldContent = oldContentString,
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

        public void FinalizeGetNewHtmlContent(Java.Lang.String content)
        {
            newContent = (string)content;
            newGetHtmlContentInterfaceSemaphore.Release();
        }

        public void FinalizeGetOldHtmlContent(Java.Lang.String content)
        {
            oldContent = (string)content;
            oldGetHtmlContentInterfaceSemaphore.Release();
        }

        #endregion

        #region Javascript interfaces

        class GetHtmlContentInterface : Java.Lang.Object
        {
            public const string NewContentJavascript = "javascript:window.GetHtmlContentInterface.getNewHtmlContent('<html>'+document.getElementsByTagName('html')[0].innerHTML+'</html>');";

            public const string OldContentJavascript = "javascript:window.GetHtmlContentInterface.getOldHtmlContent('<html>'+document.getElementsByTagName('html')[0].innerHTML+'</html>');";

            readonly ContentView view;

            public GetHtmlContentInterface(ContentView customWebViewRenderer)
            {
                view = customWebViewRenderer;
            }

            public GetHtmlContentInterface(IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
                : base(handle, transfer)
            {
            }

            [Export("getNewHtmlContent")]
            [JavascriptInterface]
            public void GetNewHtmlContent(Java.Lang.String htmlContent)
            {
                view.FinalizeGetNewHtmlContent(htmlContent);
            }

            [Export("getOldHtmlContent")]
            [JavascriptInterface]
            public void GetOldHtmlContent(Java.Lang.String htmlContent)
            {
                view.FinalizeGetOldHtmlContent(htmlContent);
            }
        }

        #endregion
    }
}