//
// Project: Mark5.Mobile.IOS
// File: ContentView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class ContentView : ComposeDocumentView, IWKNavigationDelegate, IUIGestureRecognizerDelegate, IWKScriptMessageHandler
    {
        UIButton expandButton;

        WKWebView newContentWebView;
        WKWebView oldContentWebView;

        string oldContent;
        bool oldContentLoaded;

        Dictionary<UIView, NSLayoutConstraint[]> constraintsStash;
        NSLayoutConstraint zeroHeightConstraint;
        NSLayoutConstraint minimumHeightConstraint;

        const string EditableContentClass = "content_c176f8ef-2579-4f1f-86c1-f289beaba2ae";
        const string TemplateElementClass = "template_75bb41fd-4984-43f5-b61d-3dbbe87bca21";

        const string DefaultEditContent = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">
                                            <html>
                                                <head>
                                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0 "">
                                                </head>
                                                <body>
                                                    <div class=""" + EditableContentClass + @""" contenteditable=""true"" style=""width: 100%"" onfocus=""editableContentFocused()""><br></div>
                                                    <div class=""" + TemplateElementClass + @""" style=""outline: 0px solid transparent""></div>
                                                    <script>
                                                    function editableContentFocused() {
                                                        webkit.messageHandlers.editableContentFocusedHandler.postMessage("""");
                                                    }
                                                    </script>
                                                </body>
                                            </html>";


        const string GetWebContentJs = "document.documentElement.outerHTML;";
        const string GetPlainTextContentJs = "document.body.innerText;";

        public ContentView()
        {
            constraintsStash = new Dictionary<UIView, NSLayoutConstraint[]>();

            InitializeNewContentControls();
            InitializePreviousContentControls();
        }

        void InitializeNewContentControls()
        {
            var preferences = new WKPreferences();
            preferences.JavaScriptCanOpenWindowsAutomatically = false;
            preferences.JavaScriptEnabled = true;

            var contentController = new WKUserContentController();
            contentController.AddScriptMessageHandler(this, "editableContentFocusedHandler");

            var configuration = new WKWebViewConfiguration();
            configuration.Preferences = preferences;
            configuration.UserContentController = contentController;
            configuration.SuppressesIncrementalRendering = false;
            configuration.AllowsInlineMediaPlayback = false;

            newContentWebView = new WKWebView(CGRect.Empty, configuration);
            newContentWebView.TranslatesAutoresizingMaskIntoConstraints = false;
            newContentWebView.Opaque = false;

            newContentWebView.ScrollView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            newContentWebView.ScrollView.ScrollEnabled = false;

            AddSubview(newContentWebView);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                    NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                    NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, 200)
                });

            newContentWebView.LoadHtmlString(DefaultEditContent, null);
        }

        void InitializePreviousContentControls()
        {
            expandButton = UIButton.FromType(UIButtonType.System);
            expandButton.SetTitle(Localization.GetString("show_original_message"), UIControlState.Normal);
            expandButton.TranslatesAutoresizingMaskIntoConstraints = false;
            expandButton.TitleLabel.Font = Theme.DefaultFont;
            expandButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
            expandButton.Opaque = false;
            expandButton.Hidden = true;
            expandButton.TouchUpInside += ExpandButton_Tapped;

            AddSubview(expandButton);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, newContentWebView, NSLayoutAttribute.Bottom, 1.0f, 0),
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, 2*HorizontalMargin),
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -2*HorizontalMargin)
            });

            var preferences = new WKPreferences();
            preferences.JavaScriptCanOpenWindowsAutomatically = false;
            preferences.JavaScriptEnabled = true;

            var configuration = new WKWebViewConfiguration();
            configuration.Preferences = preferences;
            configuration.SuppressesIncrementalRendering = false;
            configuration.AllowsInlineMediaPlayback = false;

            oldContentWebView = new WKWebView(CGRect.Empty, configuration);
            oldContentWebView.TranslatesAutoresizingMaskIntoConstraints = false;
            oldContentWebView.Hidden = true;
            oldContentWebView.Opaque = false;
            var oldContentWebViewNavigationDelegate = new OldContentWebViewNavigationDelegate();
            oldContentWebViewNavigationDelegate.HeightChangedAction = HandleHeightChanged;
            oldContentWebView.NavigationDelegate = oldContentWebViewNavigationDelegate;

            oldContentWebView.ScrollView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            oldContentWebView.ScrollView.ScrollEnabled = false;
            AddSubview(oldContentWebView);

            minimumHeightConstraint = NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, 0.5f);
            zeroHeightConstraint = NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, 0.0f);

            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, expandButton, NSLayoutAttribute.Bottom, 1.0f, 0),
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin),
                zeroHeightConstraint
            });
        }

        void HandleHeightChanged(nfloat height)
        {
            minimumHeightConstraint.Constant = height;
        }

        #region Public methods

        public override async Task RefreshView()
        {
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
                expandButton.Hidden &= PreviousDocument == null;
            }

        }

        public override async Task UpdateDocument()
        {
            Document.HtmlBody = await RetrieveCombinedText();
            DocumentPreview.Preview = await GetPreview(Document.HtmlBody);
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

        #region Private methods

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

                oldContentWebView.LoadHtmlString(oldContent, null);
                oldContentLoaded = true;
            }
        }

        string GetHtmlHeader()
        {
            var date = DateTime.Now.ToString(); //TODO modify

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

        async Task<string> RetrieveCombinedText()
        {
            var newContentString = (await newContentWebView.EvaluateJavaScriptAsync(GetWebContentJs) as NSString);

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

        static async Task<string> GetPreview(string htmlText)
        {
            var htmlParser = new HtmlParser();
            var newContentParsed = await htmlParser.ParseAsync(htmlText);
            var textContent = newContentParsed.Body.TextContent;
            return textContent.SafeSubstring(0, 300).TrimStart();
        }

        async Task SetWebContentPart(string parentElementClass, ContentType contentType, string content)
        {
            var currentContent = (await newContentWebView.EvaluateJavaScriptAsync(GetWebContentJs) as NSString);

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

            newContentWebView.StopLoading();
            newContentWebView.LoadHtmlString(textWriter.ToString(), null);
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

        #endregion

        #region Event handlers

        async void ExpandButton_Tapped(object sender, EventArgs e)
        {
            if (oldContentWebView.Hidden)
            {
                expandButton.SetTitle(Localization.GetString("hide_original_message"), UIControlState.Normal);
                oldContentWebView.Hidden = false;

                await LoadOldContent();

                RemoveConstraint(zeroHeightConstraint);
                AddConstraint(minimumHeightConstraint);

                oldContentWebView.RestoreConstaints(constraintsStash);

                constraintsStash.Clear();

                LayoutIfNeeded();
            }
            else
            {
                expandButton.SetTitle(Localization.GetString("show_original_message"), UIControlState.Normal);
                oldContentWebView.Hidden = true;

                constraintsStash = oldContentWebView.BackupConstaints();

                RemoveConstraint(minimumHeightConstraint);
                AddConstraint(zeroHeightConstraint);

                LayoutIfNeeded();
            }
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            if (message.Name == "editableContentFocusedHandler")
            {
                HandleScrollToView(this, EventArgs.Empty);
            }
        }

        #endregion

        class OldContentWebViewNavigationDelegate : WKNavigationDelegate
        {
            public Action<nfloat> HeightChangedAction
            {
                get;
                set;
            }

            public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
            {
                BeginInvokeOnMainThread(async () =>
                {
                    //Not sure why it does not work withouth the following line
                    await webView.EvaluateJavaScriptAsync("");

                    if (HeightChangedAction != null)
                    {
                        HeightChangedAction(webView.ScrollView.ContentSize.Height);
                    }
                });
            }
        }

    }
}
