using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using HtmlAgilityPack;
using MailBee.Html;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using Android.App;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class ContentView : ComposeDocumentView
    {
        readonly CustomWebView newContentWebView;
        readonly AppCompatButton showOldContentButton;
        readonly CustomWebView oldContentWebView;
        readonly Context context;

        readonly SemaphoreSlim newContentSemaphore = new SemaphoreSlim(1, 1);
        readonly SemaphoreSlim oldContentSemaphore = new SemaphoreSlim(1, 1);

        bool oldContentShown;

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
            var customWebViewClient = new CustomWebViewClient();
            customWebViewClient.PageFinishedLoading += (sender, e) => { newContentSemaphore.Release(); };
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
            oldContentWebViewClient.PageFinishedLoading += (sender, e) => { oldContentSemaphore.Release(); };
            oldContentWebView.SetWebViewClient(oldContentWebViewClient);
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

            if (RestoreWorkingCopy)
            {
                await newContentSemaphore.WaitAsync();
                await newContentWebView.LoadHtml(context, Document.HtmlBody, HtmlProcessingConfiguration.DefaultForEditing);
            }
            else
            {
                var previousDocumentLoaded = false;

                await newContentSemaphore.WaitAsync();
                newContentWebView.LoadEditor(context);

                if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepTextAndAttachments ||
                        DocumentCreationModeFlag == DocumentCreationModeFlag.Reply && CopyToNewOption == CopyToNewOption.None ||
                        DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll && CopyToNewOption == CopyToNewOption.None ||
                        DocumentCreationModeFlag == DocumentCreationModeFlag.Forward && CopyToNewOption == CopyToNewOption.None)
                {
                    if (PreviousDocumentPreview != null && !string.IsNullOrWhiteSpace(PreviousDocument?.HtmlBody))
                    {
                        var config = HtmlProcessingConfiguration.DefaultForEditing;
                        config.InjectReplyHeader = true;
                        config.ReplyHeaderParameters = GetReplyHeaderParameters(PreviousDocumentPreview);

                        await oldContentSemaphore.WaitAsync();
                        await oldContentWebView.LoadHtml(context, PreviousDocument.HtmlBody, config);

                        previousDocumentLoaded = true;
                    }
                    else if (PreviousDocumentPreview != null && !string.IsNullOrWhiteSpace(PreviousDocument?.PlainTextBody))
                    {
                        var config = PlainTextProcessingConfiguration.DefaultForEditing;
                        config.InjectReplyHeader = true;
                        config.ReplyHeaderParameters = GetReplyHeaderParameters(PreviousDocumentPreview);

                        await oldContentSemaphore.WaitAsync();
                        await oldContentWebView.LoadPlainText(context, PreviousDocument.PlainTextBody, config);

                        previousDocumentLoaded = true;
                    }

                    showOldContentButton.Visibility = previousDocumentLoaded ? ViewStates.Visible : ViewStates.Gone;
                }
            }
        }

        public override async Task UpdateDocument()
        {
            Document.HtmlBody = await GetContent();
        }

        public async Task InsertTemplate(Template template)
        {
            //
        }

        public async Task InsertLocalTemplate(string localTemplate)
        {
            //
        }

        #endregion

        #region State related

        async Task RestoreState()
        {
            var contentViewState = (ContentViewState)State;
            oldContentWebView.Visibility = contentViewState.OldContentWebViewVisibility;
            showOldContentButton.Visibility = contentViewState.ShowOldContentButtonVisibility;
            showOldContentButton.Text = contentViewState.ShowOldContentButtonText;

            await newContentSemaphore.WaitAsync();
            await newContentWebView.LoadHtml(context, contentViewState.NewContent, HtmlProcessingConfiguration.Disabled);

            await oldContentSemaphore.WaitAsync();
            await oldContentWebView.LoadHtml(context, contentViewState.OldContent, HtmlProcessingConfiguration.Disabled);
        }

        public override IComposeDocumentViewState GetState()
        {
            //var newContentString = AsyncHelpers.RunSync(() =>
            //{
            //    return GetNewContent();
            //});
            var oldContentString = AsyncHelpers.RunSync(() =>
            {
                return GetOldContent();
            });

            return new ContentViewState
            {
                //NewContent = newContentString,
                //OldContent = oldContentString,
                OldContentWebViewVisibility = oldContentWebView.Visibility,
                ShowOldContentButtonVisibility = showOldContentButton.Visibility,
                ShowOldContentButtonText = showOldContentButton.Text,
            };
        }

        class ContentViewState : IComposeDocumentViewState
        {
            public string NewContent { get; set; }
            public string OldContent { get; set; }
            public ViewStates OldContentWebViewVisibility { get; set; }
            public ViewStates ShowOldContentButtonVisibility { get; set; }
            public string ShowOldContentButtonText { get; set; }
        }

        #endregion

        async Task<string> GetContent()
        {
            var newContent = await GetNewContent();
            newContent = await CleanContent(newContent);

            var oldContent = await GetOldContent();
            if (!string.IsNullOrWhiteSpace(oldContent))
            {
                oldContent = await CleanContent(oldContent);
                var mergedContent = await MergeContent(newContent, oldContent);
                return mergedContent;
            }

            return newContent;
        }

        async Task<string> GetNewContent()
        {
            try
            {
                await newContentSemaphore.WaitAsync();
                return await newContentWebView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
            }
            finally
            {
                newContentSemaphore.Release();
            }
        }

        async Task<string> GetOldContent()
        {
            try
            {
                await oldContentSemaphore.WaitAsync();
                return await oldContentWebView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
            }
            finally
            {
                oldContentSemaphore.Release();
            }
        }

        Task<string> CleanContent(string content)
        {
            return Task.Run(() =>
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(content);

                var headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");
                headNode?.ChildNodes.FirstOrDefault(n => n.Name == "link" && n.Attributes.Any(attr => attr.Name == "id" && attr.Value == "fonts"))?.Remove();
                headNode?.ChildNodes.FirstOrDefault(n => n.Name == "meta" && n.Attributes.Any(attr => attr.Name == "id" && attr.Value == "viewport"))?.Remove();
                headNode?.ChildNodes.FirstOrDefault(n => n.Name == "style" && n.Attributes.Any(attr => attr.Name == "id" && attr.Value == "style1"))?.Remove();

                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
                bodyNode?.Attributes.FirstOrDefault(attr => attr.Name == "contentEditable")?.Remove();
                bodyNode?.ChildNodes.FirstOrDefault(n => n.Name == "div" && n.Attributes.Any(attr => attr.Name == "id" && attr.Value == "headerpadding"))?.Remove();

                var editorNode = bodyNode?.SelectSingleNode("//div[@id='editor']");
                editorNode?.Attributes.FirstOrDefault(attr => attr.Name == "contentEditable")?.Remove();

                var html = htmlDocument.DocumentNode.OuterHtml;

                html = PreMailer.Net.PreMailer.MoveCssInline(html, true, null, null, true, true).Html;

                var p = new Processor();
                p.Dom.OuterHtml = html;
                html = p.Dom.ProcessToString(RuleSet.GetSafeHtmlRules(), null);

                return html;
            });
        }

        Task<string> MergeContent(string newContent, string oldContent)
        {
            return Task.Run(() =>
            {
                var newHtmlDocument = new HtmlDocument();
                newHtmlDocument.LoadHtml(newContent);
                var oldHtmlDocument = new HtmlDocument();
                oldHtmlDocument.LoadHtml(oldContent);

                string html;
                using (var sr = new StreamReader(context.Assets.Open("blank.html")))
                    html = sr.ReadToEnd();

                var mergedHtmlDocument = new HtmlDocument();
                mergedHtmlDocument.LoadHtml(html);

                var newNode = mergedHtmlDocument.DocumentNode.SelectSingleNode("//div[@id='new']");
                newNode.AppendChildren(newHtmlDocument.DocumentNode.SelectSingleNode("//body").ChildNodes);
                newNode.Attributes.Remove("id");
                var oldNode = mergedHtmlDocument.DocumentNode.SelectSingleNode("//div[@id='old']");
                oldNode.AppendChildren(oldHtmlDocument.DocumentNode.SelectSingleNode("//body").ChildNodes);
                oldNode.Attributes.Remove("id");

                return mergedHtmlDocument.DocumentNode.OuterHtml;
            });
        }

        string[] GetReplyHeaderParameters(DocumentPreview documentPreview)
        {
            var from = GetAddressTextFromPreviousDocument(documentPreview, DocumentAddressType.From);
            var date = documentPreview.DateReceivedTimestamp
                                      .ConvertTimestampMillisecondsToDateTime()
                                      .ConvertUtcToUserTime()
                                      .ConvertDateTimeToTimestampMilliseconds()
                                      .FormatUserTimestampAsTimeAndDateString(context);
            var to = GetAddressTextFromPreviousDocument(documentPreview, DocumentAddressType.To, DocumentAddressType.Cc);
            var subject = documentPreview.Subject;

            return new[] { from, date, to, subject };
        }

        static string GetAddressTextFromPreviousDocument(DocumentPreview documentPreview, params DocumentAddressType[] addressTypes)
        {
            var sb = new StringBuilder();
            var addresses = documentPreview.Addresses.Where(da => addressTypes.Contains(da.AddressType)).ToArray();
            for (var i = 0; i < addresses.Length; i++)
            {
                var hasName = !string.IsNullOrWhiteSpace(addresses[i].Name);
                if (hasName)
                    sb.Append(addresses[i].Name).Append(" &lt;");
                sb.Append(addresses[i].Address);
                if (hasName)
                    sb.Append("&gt;");
                if (i < addresses.Length - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }
    }
}