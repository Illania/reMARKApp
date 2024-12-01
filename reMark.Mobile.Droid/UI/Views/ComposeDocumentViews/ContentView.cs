using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using HtmlAgilityPack;
using MailBee.Html;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Model;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Views.Common;
using reMark.Mobile.Droid.Utilities;
using reMark.Mobile.Droid.Utilities.Extensions;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public partial class ContentView : ComposeDocumentView
    {
        readonly CustomWebView newContentWebView;
        readonly AppCompatButton showOldContentButton;
        readonly CustomWebView oldContentWebView;
        readonly Context context;
        readonly FormattingView formattingView;

        readonly SemaphoreSlim newContentSemaphore = new SemaphoreSlim(1, 1);
        readonly SemaphoreSlim oldContentSemaphore = new SemaphoreSlim(1, 1);

        readonly Action formattingViewVisibilityChangeAction;

        bool oldContentShown;

        public ContentView(Context context, FormattingView formattingView, Action<View, int> moveViewToCaretAction, Action formattingViewVisibilityChangeAction)
            : base(context)
        {
            this.context = context;
            this.formattingViewVisibilityChangeAction = formattingViewVisibilityChangeAction;
            Orientation = Vertical;
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, 0)
            {
                Weight = 1
            };
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            this.formattingView = formattingView;

            formattingView.BoldClicked += FormattingView_BoldClicked;
            formattingView.ItalicClicked += FormattingView_ItalicClicked;
            formattingView.UnderlineClicked += FormattingView_UnderlineClicked;

            newContentWebView = new CustomWebView(context, moveViewToCaretAction)
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

            oldContentWebView = new CustomWebView(context, moveViewToCaretAction)
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
            if (RestoreWorkingCopy)
            {
                await newContentSemaphore.WaitAsync();
                await newContentWebView.LoadHtml(context, Document.HtmlBody, HtmlProcessingConfiguration.DefaultForEditing);
            }
            else
            {
                await newContentSemaphore.WaitAsync();
                newContentWebView.LoadEditor(context);
                
                if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption.HasFlag(CopyToNewOption.Content))
                    await LoadPreviousDocumentBody(oldContentSemaphore, oldContentWebView);
                else if (PreviousDocument != null && PreviousDocumentDirection == DocumentDirection.Draft)
                    await LoadPreviousDocumentBody(newContentSemaphore, newContentWebView);
                else if (PreviousDocumentPreview != null &&
                         (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply && CopyToNewOption == CopyToNewOption.None ||
                          DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll && CopyToNewOption == CopyToNewOption.None ||
                          DocumentCreationModeFlag == DocumentCreationModeFlag.Forward && CopyToNewOption == CopyToNewOption.None))
                {
                    await ProcessReplyForward();
                }
            }
        }
        
        private async Task ProcessReplyForward()
        {
            var previousDocumentLoaded = false;
            
            if (!string.IsNullOrWhiteSpace(PreviousDocument?.HtmlBody))
            {
                var config = HtmlProcessingConfiguration.DefaultForEditing;
                config.InjectReplyHeader = true;
                config.ReplyHeaderParameters = HtmlUtilities.GetReplyHeaderParameters(Context, PreviousDocumentPreview, PreviousDocument);

                await oldContentSemaphore.WaitAsync();
                await oldContentWebView.LoadHtml(context, PreviousDocument.HtmlBody, config);

                previousDocumentLoaded = true;
            }
            else if (!string.IsNullOrWhiteSpace(PreviousDocument?.PlainTextBody))
            {
                var config = PlainTextProcessingConfiguration.DefaultForEditing;
                config.InjectReplyHeader = true;
                config.ReplyHeaderParameters = HtmlUtilities.GetReplyHeaderParameters(Context, PreviousDocumentPreview, PreviousDocument);

                await oldContentSemaphore.WaitAsync();
                await oldContentWebView.LoadPlainText(context, PreviousDocument.PlainTextBody, config);

                previousDocumentLoaded = true;
            }

            showOldContentButton.Visibility = previousDocumentLoaded ? ViewStates.Visible : ViewStates.Gone;
        }

        private async Task LoadPreviousDocumentBody(SemaphoreSlim semaphoreSlim, CustomWebView webView)
        {
            if (!string.IsNullOrWhiteSpace(PreviousDocument?.HtmlBody))
            {
                await semaphoreSlim.WaitAsync();
                await webView.LoadHtml(context, PreviousDocument.HtmlBody, HtmlProcessingConfiguration.DefaultForEditing);
            }
            else if (!string.IsNullOrWhiteSpace(PreviousDocument?.PlainTextBody))
            {
                await semaphoreSlim.WaitAsync();
                await webView.LoadPlainText(context, PreviousDocument.PlainTextBody,
                    PlainTextProcessingConfiguration.DefaultForEditing);
            }
        }

        public override async Task UpdateDocument()
        {
            Document.HtmlBody = await AsyncHelpers.RunOnUiThreadAsync((Activity)Context, () => GetContentAsync());
        }

        public async Task InsertTemplate(Template template, bool initializing)
        {
            string insertTemplateJs;

            if (initializing)
            {
                using (var sr = new StreamReader(context.Assets.Open("initTemplate.js")))
                    insertTemplateJs = sr.ReadToEnd();
            }
            else
            {
                using (var sr = new StreamReader(context.Assets.Open("insertTemplate.js")))
                    insertTemplateJs = sr.ReadToEnd();
            }

            if (template.ContentType == ContentType.PlainText)
            {
                var templateText = Regex.Replace(template.Content, @"\r\n?|\n", "\\n", RegexOptions.Multiline);
                insertTemplateJs = HtmlUtilities.ProcessWebTemplate(insertTemplateJs, "text", template.Id, templateText);
            }
            if (template.ContentType == ContentType.Html)
            {
                var templateText = Regex.Replace(template.Content, @"\r\n?|\n", " ", RegexOptions.Multiline);
                insertTemplateJs = HtmlUtilities.ProcessWebTemplate(insertTemplateJs, "html", template.Id, templateText);
            }

            var result = await newContentWebView.EvaluateJavaScriptAsync(insertTemplateJs);
        }


        public async Task InsertLocalTemplate(string localTemplate)
        {
            string insertTemplateJs;
            using (var sr = new StreamReader(context.Assets.Open("initTemplate.js")))
                insertTemplateJs = sr.ReadToEnd();

            var localTemplateText = Regex.Replace(localTemplate, @"\r\n?|\n", "\\n", RegexOptions.Multiline);

            insertTemplateJs = HtmlUtilities.ProcessWebTemplate(insertTemplateJs, "text", "local", localTemplateText);
            await newContentWebView.EvaluateJavaScriptAsync(insertTemplateJs);
        }

        public async Task InsertPlainText(string content)
        {
            await newContentSemaphore.WaitAsync();
            await newContentWebView.LoadPlainText(context, content, PlainTextProcessingConfiguration.DefaultForEditing);
        }

        #endregion

        async Task<string> GetContentAsync()
        {
            var newContent = await GetNewContentAsync();
            newContent = await CleanContentAsync(newContent);

            var oldContent = await GetOldContentAsync();
            if (!string.IsNullOrWhiteSpace(oldContent))
            {
                oldContent = await CleanContentAsync(oldContent);
                var mergedContent = await MergeContentAsync(newContent, oldContent);
                return mergedContent;
            }

            return newContent;
        }

        async Task<string> GetNewContentAsync()
        {
            try
            {
                await newContentSemaphore.WaitAsync();
                var result = await newContentWebView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
                return result;
            }
            finally
            {
                newContentSemaphore.Release();
            }
        }

        async Task<string> GetOldContentAsync()
        {
            try
            {
                await oldContentSemaphore.WaitAsync();
                var result = await oldContentWebView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
                return result;
            }
            finally
            {
                oldContentSemaphore.Release();
            }
        }

        Task<string> CleanContentAsync(string content)
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

                html = PreMailer.Net.PreMailer.MoveCssInline(html, true, null, null, 
                    true, true).Html;
                
                html = ReplaceTextDecorationAttribute(html);

                var p = new Processor();
                p.Dom.OuterHtml = html;
                html = p.Dom.ProcessToString(RuleSet.GetSafeHtmlRules(), null);

                return html;
            });
        }


        /// <summary>
        /// Replaces instances of the CSS property 'text-decoration-line: line-through;' 
        /// with the shorthand equivalent 'text-decoration: line-through;' in the provided HTML string
        /// to avoid a limitation related to the OpenXml document format when converting html to rtf format on server side,
        /// which removes the strikethrough from lines that are marked with 'text-decoration-line: line-through;' attribute.
        /// Refernece ticket https://nordicit.atlassian.net/browse/M5APP-1621
        /// </summary>
        /// <param name="html">A string containing HTML content where the CSS replacements will be made.</param>
        /// <returns>A string with the specified CSS property replaced by its shorthand form.</returns>
        private static string ReplaceTextDecorationAttribute(string html)
        {
            const string pattern = @"text-decoration-line:\s*line-through;";
            const string replacement = "text-decoration: line-through;";
            html = TextDecorationLineRegex().Replace(html, replacement);
            return html;
        }

        Task<string> MergeContentAsync(string newContent, string oldContent)
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

        #region Formatting related

        private void FormattingView_BoldClicked(object sender, EventArgs e)
        {
            newContentWebView.SetBold();
            oldContentWebView.SetBold();
        }

        private void FormattingView_ItalicClicked(object sender, EventArgs e)
        {
            newContentWebView.SetItalic();
            oldContentWebView.SetItalic();
        }

        private void FormattingView_UnderlineClicked(object sender, EventArgs e)
        {
            newContentWebView.SetUnderline();
            oldContentWebView.SetUnderline();
        }
        
        public async Task LoadMergedContent()
        {
            var mergedContent = await GetContentAsync();
            await newContentSemaphore.WaitAsync();
            await newContentWebView.LoadHtml(context, mergedContent, HtmlProcessingConfiguration.DefaultForEditing);
        }

        [GeneratedRegex(@"text-decoration-line:\s*line-through;", RegexOptions.IgnoreCase, "en-RU")]
        private static partial Regex TextDecorationLineRegex();

        #endregion
    }
}
