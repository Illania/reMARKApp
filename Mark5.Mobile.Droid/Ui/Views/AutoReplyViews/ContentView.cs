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
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Droid.Utilities.Extensions;

namespace Mark5.Mobile.Droid.Ui.Views.AutoReplyViews
{
    public class ContentView : AutoReplySubView
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

        }


        #region Public methods

        public override async Task RefreshView()
        {
            await newContentSemaphore.WaitAsync();
            await newContentWebView.LoadHtml(context, AutoReplyRule.ReplyText.ToString(), HtmlProcessingConfiguration.DefaultForEditing);
        }

        public override async Task UpdateAutoReply()
        {
            AutoReplyRule.ReplyText = await AsyncHelpers.RunOnUiThreadAsync((Activity)Context, () => GetContentAsync());
        }

        #endregion

        async Task<string> GetContentAsync()
        {
            var newContent = await GetNewContentAsync();
            newContent = await CleanContentAsync(newContent);

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

                html = PreMailer.Net.PreMailer.MoveCssInline(html, true, null, null, true, true).Html;

                var p = new Processor();
                p.Dom.OuterHtml = html;
                html = p.Dom.ProcessToString(RuleSet.GetSafeHtmlRules(), null);

                return html;
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

        #endregion
    }
}