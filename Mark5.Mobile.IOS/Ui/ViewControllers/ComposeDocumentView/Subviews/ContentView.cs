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
using System.Threading;
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
    public class ContentView : ComposeDocumentSubView, IWKNavigationDelegate, IUIGestureRecognizerDelegate, IWKScriptMessageHandler
    {
        UIButton expandButton;

        WKWebView newContentWebView;
        WKWebView oldContentWebView;

        SeparatorSubView separatorBeforeExpand;
        SeparatorSubView separatorAfterExpand;

        string oldContent;
        bool oldContentLoaded;

        Dictionary<UIView, NSLayoutConstraint[]> constraintsStash;

        NSLayoutConstraint newContentHeightConstraint;
        NSLayoutConstraint newContentWidthConstraint;

        NSLayoutConstraint oldContentZeroHeightConstraint;
        NSLayoutConstraint oldContentHeightConstraint;

        nfloat centerGestureStartY;

        SemaphoreSlim newContentLoadingSemaphore;

        IDisposable newContentObserver;
        IDisposable oldContentObserver;

        const string EditableContentClass = "content_c176f8ef-2579-4f1f-86c1-f289beaba2ae";

        const string DefaultEditContent = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">
                                            <html>
                                                <head>
                                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0 "">
                                                </head>
                                                <body>
                                                    <div id=""editable-one"" class=""" + EditableContentClass + @""" contenteditable=""true"" style=""width: 100%"" onfocus=""editableContentFocused()""><br></div>
                                                    <script>
                                                    function editableContentFocused() {
                                                        webkit.messageHandlers.editableContentFocusedHandler.postMessage("""");
                                                    }

                                                    function editableContentInput(evt) {                                                         webkit.messageHandlers.editableContentInputHandler.postMessage("""");                                                     }                                                      var node = document.getElementById('editable-one');                                                     node.addEventListener(""input"", editableContentInput, false); 
                                                    </script >
                                                </body >
                                            </html>";


        const string GetWebContentJs = "document.documentElement.outerHTML;";
        const string GetPlainTextContentJs = "document.body.innerText;";

        public ContentView()
        {
            constraintsStash = new Dictionary<UIView, NSLayoutConstraint[]>();
            newContentLoadingSemaphore = new SemaphoreSlim(0);
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
            contentController.AddScriptMessageHandler(this, "editableContentInputHandler");

            var configuration = new WKWebViewConfiguration();
            configuration.Preferences = preferences;
            configuration.UserContentController = contentController;
            configuration.SuppressesIncrementalRendering = false;
            configuration.AllowsInlineMediaPlayback = false;

            newContentWebView = new WKWebView(CGRect.Empty, configuration);
            newContentWebView.TranslatesAutoresizingMaskIntoConstraints = false;
            newContentWebView.Opaque = false;

            newContentWebView.ScrollView.ScrollsToTop = false;
            newContentWebView.ScrollView.Bounces = false;
            newContentWebView.ScrollView.ScrollEnabled = false;
            newContentWebView.ScrollView.UserInteractionEnabled = true;
            newContentObserver = newContentWebView.ScrollView.AddObserver("contentSize", NSKeyValueObservingOptions.New, obj => UpdateWebViewSize(newContentWebView, newContentHeightConstraint, newContentWidthConstraint));
            var tapRecognizer = new UITapGestureRecognizer(HandleTap);
            tapRecognizer.Delegate = this;
            newContentWebView.ScrollView.AddGestureRecognizer(tapRecognizer);
            var navigationDelegate = new WebViewNavigationDelegate();
            navigationDelegate.DidFinishNavigationAction = () =>
            {
                newContentLoadingSemaphore.Release();
            };
            newContentWebView.NavigationDelegate = navigationDelegate;

            newContentHeightConstraint = NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, 200f);
            newContentWidthConstraint = NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, 1f);

            ContainerView.AddSubview(newContentWebView);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                    NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                    NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                    newContentHeightConstraint,
                    newContentWidthConstraint
                });

            newContentWebView.LoadHtmlString(DefaultEditContent, null);
        }

        protected override void Dispose(bool disposing)
        {
            newContentObserver.Dispose();
            oldContentObserver.Dispose();
            base.Dispose(disposing);
        }

        void InitializePreviousContentControls()
        {
            separatorBeforeExpand = new SeparatorSubView();
            separatorBeforeExpand.TranslatesAutoresizingMaskIntoConstraints = false;
            separatorBeforeExpand.Hidden = true;
            ContainerView.AddSubview(separatorBeforeExpand);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(separatorBeforeExpand, NSLayoutAttribute.Top, NSLayoutRelation.Equal, newContentWebView, NSLayoutAttribute.Bottom, 1f, 0),
                NSLayoutConstraint.Create(separatorBeforeExpand, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, 0),
                NSLayoutConstraint.Create(separatorBeforeExpand, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, 0)
            });

            expandButton = UIButton.FromType(UIButtonType.System);
            expandButton.SetTitle(Localization.GetString("show_original_message"), UIControlState.Normal);
            expandButton.TranslatesAutoresizingMaskIntoConstraints = false;
            expandButton.TitleLabel.Font = Theme.DefaultFont;
            expandButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
            expandButton.Opaque = false;
            expandButton.Hidden = true;
            expandButton.TouchUpInside += ExpandButton_Tapped;

            ContainerView.AddSubview(expandButton);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, separatorBeforeExpand, NSLayoutAttribute.Bottom, 1f, 0),
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, 2*HorizontalMargin),
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -2*HorizontalMargin)
            });

            separatorAfterExpand = new SeparatorSubView();
            separatorAfterExpand.Hidden = true;
            separatorAfterExpand.TranslatesAutoresizingMaskIntoConstraints = false;
            ContainerView.AddSubview(separatorAfterExpand);

            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(separatorAfterExpand, NSLayoutAttribute.Top, NSLayoutRelation.Equal, expandButton, NSLayoutAttribute.Bottom, 1f, 0),
                NSLayoutConstraint.Create(separatorAfterExpand, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, 0),
                NSLayoutConstraint.Create(separatorAfterExpand, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, 0)
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
            var oldContentWebViewNavigationDelegate = new WebViewNavigationDelegate();
            oldContentWebView.NavigationDelegate = oldContentWebViewNavigationDelegate;

            oldContentWebView.ScrollView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            oldContentWebView.ScrollView.ScrollEnabled = false;
            oldContentObserver = oldContentWebView.ScrollView.AddObserver("contentSize", NSKeyValueObservingOptions.New, obj => UpdateWebViewSize(oldContentWebView, oldContentHeightConstraint, null));
            ContainerView.AddSubview(oldContentWebView);

            oldContentHeightConstraint = NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0.5f);
            oldContentZeroHeightConstraint = NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0f);

            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, separatorAfterExpand, NSLayoutAttribute.Bottom, 1f, 0),
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin),
                oldContentZeroHeightConstraint
            });
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
                separatorBeforeExpand.Hidden = expandButton.Hidden;
            }

        }

        public override async Task UpdateDocument()
        {
            Document.HtmlBody = await RetrieveCombinedText();
            DocumentPreview.Preview = await GetPreview(Document.HtmlBody);
        }

        public async Task InsertTemplate(Template template)
        {
            await SetWebContentPart(EditableContentClass, template.ContentType, GetContentWithSpace(template.ContentType, template.Content));
        }

        public async Task InsertLocalTemplate(string localTemplate)
        {
            await SetWebContentPart(EditableContentClass, ContentType.PlainText, GetContentWithSpace(ContentType.PlainText, localTemplate));
        }

        string GetContentWithSpace(ContentType contentType, string content)
        {
            if (contentType == ContentType.Html)
            {
                return "<br><br><br>" + content;
            }
            if (contentType == ContentType.PlainText)
            {
                return "\n\n\n\n" + content;
            }

            throw new ArgumentException("Invalid content type");
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
            var date = PreviousDocumentPreview.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime()
                         .ConvertUtcToServerTime()
                         .ConvertDateTimeToTimestampMilliseconds()
                        .FormatServerTimestampAsCompactLongDateTimeString();

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
            return string.Empty;
        }

        async Task<string> RetrieveCombinedText()
        {
            var newContentString = (await newContentWebView.EvaluateJavaScriptAsync(GetWebContentJs) as NSString).ToString();
            var newContentProcessedString = await ProcessRetrievedContent(new HtmlParser(), newContentString);

            await LoadOldContent();

            if (!string.IsNullOrEmpty(oldContent))
            {
                var htmlParser = new HtmlParser();
                var newContentParsed = await htmlParser.ParseAsync(newContentProcessedString);
                var oldContentParsed = await htmlParser.ParseAsync(oldContent);
                var oldContentInDiv = newContentParsed.CreateElement("div");
                oldContentInDiv.InnerHtml = oldContentParsed.Body.InnerHtml;
                newContentParsed.Body.Append(oldContentInDiv);
                var textWriter = new StringWriter();
                newContentParsed.ToHtml(textWriter, HtmlMarkupFormatter.Instance);
                return textWriter.ToString();
            }

            return newContentProcessedString;
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
            await newContentLoadingSemaphore.WaitAsync();

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

            var scriptElements = currentHtmlDocument.Body.QuerySelectorAll("script");
            foreach (var scriptElement in scriptElements)
            {
                currentHtmlDocument.Body.RemoveChild(scriptElement);
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

        #endregion

        #region IUIGestureRecognizerDelegate

        [Export("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
        public bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }

        #endregion

        #region Gesture Recognizer handlers

        void HandleTap(UITapGestureRecognizer gestureRecognizer)
        {
            centerGestureStartY = gestureRecognizer.LocationInView(Superview.Superview).Y;
        }

        #endregion

        #region Event handlers

        async void ExpandButton_Tapped(object sender, EventArgs e)
        {
            if (oldContentWebView.Hidden)
            {
                expandButton.SetTitle(Localization.GetString("hide_original_message"), UIControlState.Normal);
                oldContentWebView.Hidden = false;
                separatorAfterExpand.Hidden = false;

                await LoadOldContent();

                RemoveConstraint(oldContentZeroHeightConstraint);
                AddConstraint(oldContentHeightConstraint);

                oldContentWebView.RestoreConstaints(constraintsStash);

                constraintsStash.Clear();

                LayoutIfNeeded();
            }
            else
            {
                expandButton.SetTitle(Localization.GetString("show_original_message"), UIControlState.Normal);
                oldContentWebView.Hidden = true;
                separatorAfterExpand.Hidden = true;

                constraintsStash = oldContentWebView.BackupConstaints();

                RemoveConstraint(oldContentHeightConstraint);
                AddConstraint(oldContentZeroHeightConstraint);

                LayoutIfNeeded();
            }
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            if (message.Name == "editableContentFocusedHandler")
            {
                var scrollView = Superview as UIScrollView;
                scrollView.SetContentOffset(new CGPoint(0, centerGestureStartY - scrollView.ContentInset.Top - 60), true);
                newContentWebView.ScrollView.SetContentOffset(new CGPoint(0, 0), true);
            }
            else if (message.Name == "editableContentInputHandler")
            {
                //UpdateWebViewHeight(newContentWebView, newContentHeightConstraint);
            }
        }

        void UpdateWebViewSize(WKWebView webView, NSLayoutConstraint heightConstraint, NSLayoutConstraint widthConstraint)
        {
            heightConstraint.Constant = webView.ScrollView.ContentSize.Height;
            if (widthConstraint != null)
            {
                widthConstraint.Constant = webView.ScrollView.ContentSize.Width;
                CommonConfig.Logger.Trace($"H={heightConstraint.Constant}, W={widthConstraint.Constant}");
            }
        }

        #endregion

        class WebViewNavigationDelegate : WKNavigationDelegate
        {
            public Action DidFinishNavigationAction { get; set; }

            public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
            {
                BeginInvokeOnMainThread(async () =>
                {
                    //Not sure why it does not work withouth the following line
                    await webView.EvaluateJavaScriptAsync("");

                    if (DidFinishNavigationAction != null)
                    {
                        DidFinishNavigationAction();
                    }
                });
            }
        }

    }
}
