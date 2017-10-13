using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using HtmlAgilityPack;
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
    public class ContentView : ComposeDocumentSubView, IWKNavigationDelegate, IUIGestureRecognizerDelegate, IWKScriptMessageHandler, IUIScrollViewDelegate
    {
        static readonly NSString script1 = new NSString("window.onload = function () {window.webkit.messageHandlers.sizeNotification.postMessage({justLoaded:true});};");
        static readonly NSString script2 = new NSString("window.onresize = function () {window.webkit.messageHandlers.sizeNotification.postMessage({resized:true});};");
        static readonly NSString script3 = new NSString("document.addEventListener(\"DOMContentLoaded\", function () {window.webkit.messageHandlers.sizeNotification.postMessage({domLoaded:true});});");
        static readonly NSString script4 = new NSString("var observer = new MutationObserver(function(mutations) { window.webkit.messageHandlers.mutation.postMessage({mutated:true}); }); observer.observe(document.querySelector('#editable-one'), { attributes: true, childList: true, characterData: true, subtree: true });");
        static readonly NSString script5 = new NSString("document.addEventListener(\"keypress\", function(e) {  if(e.which == 13) { window.webkit.messageHandlers.keypress.postMessage({keypressed:true}); } })");

        UIButton expandButton;

        WKWebView newContentWebView;
        WKWebView oldContentWebView;

        UIScrollView externalScrollView;

        SeparatorSubView separatorBeforeExpand;
        SeparatorSubView separatorAfterExpand;

        bool oldContentLoaded;

        bool oldContentResized;
        bool newContentResized;

        CGPoint tapLocationOldContent;
        CGPoint tapLocationNewContent;

        Dictionary<UIView, NSLayoutConstraint[]> constraintsStash = new Dictionary<UIView, NSLayoutConstraint[]>();

        NSLayoutConstraint newContentHeightConstraint;
        NSLayoutConstraint oldContentZeroHeightConstraint;
        NSLayoutConstraint oldContentHeightConstraint;

        SemaphoreSlim newContentLoadingSemaphore = new SemaphoreSlim(0, 1);
        const string NewEditableContentClass = "new_content_c176f8ef-2579-4f1f-86c1-f289beaba2ae";
        const string OldEditableContentClass = "old_content_cc4ee2cb-e18c-423a-adb2-d106a29dcbc3";

        const string DefaultEditContent = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">
                                            <html>
                                                <head>
                                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1, minimum-scale=1, maximum-scale=1"">
                                                </head>
                                                <body>
                                                    <div id=""editable-one"" class=""" + NewEditableContentClass + @""" contenteditable=""true"" style=""font-family: sans-serif; width: 100%""><br></div>
                                                </body >
                                            </html>";


        const string GetWebContentJs = "document.documentElement.outerHTML;";
        const string GetPlainTextContentJs = "document.body.innerText;";

        public ContentView(UIScrollView externalScrollView)
        {
            this.externalScrollView = externalScrollView;

            InitializeNewContentControls();
            InitializeOldContentControls();
        }

        void InitializeNewContentControls()
        {
            var preferences = new WKPreferences
            {
                MinimumFontSize = 12f,
                JavaScriptCanOpenWindowsAutomatically = false,
                JavaScriptEnabled = true
            };
            var wkscript1 = new WKUserScript(script1, WKUserScriptInjectionTime.AtDocumentEnd, true);
            var wkscript2 = new WKUserScript(script2, WKUserScriptInjectionTime.AtDocumentEnd, true);
            var wkscript3 = new WKUserScript(script3, WKUserScriptInjectionTime.AtDocumentStart, true);
            var wkscript4 = new WKUserScript(script4, WKUserScriptInjectionTime.AtDocumentEnd, true);
            var wkscript5 = new WKUserScript(script5, WKUserScriptInjectionTime.AtDocumentEnd, true);

            var userContentController = new WKUserContentController();
            userContentController.AddUserScript(wkscript1);
            userContentController.AddUserScript(wkscript2);
            userContentController.AddUserScript(wkscript3);
            userContentController.AddUserScript(wkscript4);
            userContentController.AddUserScript(wkscript5);
            userContentController.AddScriptMessageHandler(this, "sizeNotification");
            userContentController.AddScriptMessageHandler(this, "mutation");
            userContentController.AddScriptMessageHandler(this, "keypress");

            var configuration = new WKWebViewConfiguration
            {
                SuppressesIncrementalRendering = true,
                AllowsInlineMediaPlayback = false,
                UserContentController = userContentController,
                Preferences = preferences
            };

            newContentWebView = new WKWebView(CGRect.Empty, configuration)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Opaque = false
            };
            newContentWebView.ScrollView.Bounces = false;
            newContentWebView.ScrollView.BouncesZoom = false;

            var tapRecognizer = new UITapGestureRecognizer();
            tapRecognizer.AddTarget(() => HandleTapNewContent(tapRecognizer));
            tapRecognizer.Delegate = this;

            newContentWebView.ScrollView.AddGestureRecognizer(tapRecognizer);
            newContentWebView.NavigationDelegate = new WebViewNavigationDelegate { DidFinishNavigationAction = () => { newContentLoadingSemaphore.Release(); } };

            ContainerView.AddSubview(newContentWebView);
            AddConstraints(new[]
            {
                newContentHeightConstraint = NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 200f),
                NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(newContentWebView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin)
            });

            newContentWebView.LoadHtmlString(DefaultEditContent, null);
        }

        void InitializeOldContentControls()
        {
            separatorBeforeExpand = new SeparatorSubView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Hidden = true
            };
            ContainerView.AddSubview(separatorBeforeExpand);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(separatorBeforeExpand, NSLayoutAttribute.Top, NSLayoutRelation.Equal, newContentWebView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(separatorBeforeExpand, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(separatorBeforeExpand, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, 0f)
            });

            expandButton = new UIButton();
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
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, separatorBeforeExpand, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, 0f)
            });

            separatorAfterExpand = new SeparatorSubView
            {
                Hidden = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContainerView.AddSubview(separatorAfterExpand);

            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(separatorAfterExpand, NSLayoutAttribute.Top, NSLayoutRelation.Equal, expandButton, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(separatorAfterExpand, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(separatorAfterExpand, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, 0f)
            });

            var preferences = new WKPreferences
            {
                MinimumFontSize = 12f,
                JavaScriptCanOpenWindowsAutomatically = false,
                JavaScriptEnabled = true
            };
            var wkscript1 = new WKUserScript(script1, WKUserScriptInjectionTime.AtDocumentEnd, true);
            var wkscript2 = new WKUserScript(script2, WKUserScriptInjectionTime.AtDocumentEnd, true);
            var wkscript3 = new WKUserScript(script3, WKUserScriptInjectionTime.AtDocumentStart, true);
            var wkscript5 = new WKUserScript(script5, WKUserScriptInjectionTime.AtDocumentEnd, true);

            var userContentController = new WKUserContentController();
            userContentController.AddUserScript(wkscript1);
            userContentController.AddUserScript(wkscript2);
            userContentController.AddUserScript(wkscript3);
            userContentController.AddUserScript(wkscript5);
            userContentController.AddScriptMessageHandler(this, "sizeNotification");
            userContentController.AddScriptMessageHandler(this, "keypress");

            var configuration = new WKWebViewConfiguration
            {
                SuppressesIncrementalRendering = true,
                AllowsInlineMediaPlayback = false,
                UserContentController = userContentController,
                Preferences = preferences
            };
            oldContentWebView = new WKWebView(CGRect.Empty, configuration)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Hidden = true,
                Opaque = false
            };
            oldContentWebView.ScrollView.Bounces = false;
            oldContentWebView.ScrollView.BouncesZoom = false;
            oldContentWebView.ScrollView.Delegate = this;

            var tapRecognizer = new UITapGestureRecognizer();
            tapRecognizer.AddTarget(() => HandleTapOldContent(tapRecognizer));
            tapRecognizer.Delegate = this;

            oldContentWebView.ScrollView.AddGestureRecognizer(tapRecognizer);
            oldContentWebView.NavigationDelegate = new WebViewNavigationDelegate();
            oldContentHeightConstraint = NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 1f);
            oldContentZeroHeightConstraint = NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 5f); // https://xkcd.com/221/

            ContainerView.AddSubview(oldContentWebView);
            ContainerView.AddConstraints(new[]
            {
                oldContentZeroHeightConstraint,
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, separatorAfterExpand, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(oldContentWebView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        #region ScrollView

        void HandleTapOldContent(UITapGestureRecognizer tapRecognizer)
        {
            tapLocationOldContent = tapRecognizer.LocationInView(this);
        }

        void HandleTapNewContent(UITapGestureRecognizer tapRecognizer)
        {
            tapLocationNewContent = tapRecognizer.LocationInView(this);
        }

        public void OnKeyboardWillShow(nfloat keyboardHeight)
        {
            ScrollToVisibleIfNecessary(ref tapLocationNewContent, newContentWebView, keyboardHeight);
            ScrollToVisibleIfNecessary(ref tapLocationOldContent, oldContentWebView, keyboardHeight);
        }

        void ScrollToVisibleIfNecessary(ref CGPoint tapLocation, WKWebView webview, nfloat keyboardHeight)
        {
            if (tapLocation == default(CGPoint))
                return;

            var oldY = ConvertRectToView(webview.Frame, null).Y; //Position in current window

            if (oldY - UIApplication.SharedApplication.KeyWindow.Frame.Bottom + keyboardHeight + 20 > 0)
            {
                var rect = new CGRect
                {
                    Height = 40,
                    Width = 1,
                    X = ConvertPointToView(tapLocation, externalScrollView).X,
                    Y = ConvertPointToView(tapLocation, externalScrollView).Y
                };
                externalScrollView.ScrollRectToVisible(rect, true);
            }

            tapLocation = default(CGPoint);
        }

        void ScrollForEnterPressed()
        {
            var co = externalScrollView.ContentOffset;
            co.Y += 20;
            externalScrollView.SetContentOffset(co, true);
        }

        #endregion

        #region Public methods

        public override async Task InitializeView()
        {
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
                expandButton.Hidden = DocumentCreationModeFlag == DocumentCreationModeFlag.New ||
                                      DocumentCreationModeFlag == DocumentCreationModeFlag.Edit ||
                                      PreviousDocument == null;
                separatorBeforeExpand.Hidden = expandButton.Hidden;

                await LoadOldContent();
            }
        }

        public override async Task UpdateDocument()
        {
            (Document.HtmlBody, DocumentPreview.Preview) = await RetreiveContentAndPreview();
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

        #region Private methods

        async Task LoadOldContent()
        {
            if (!oldContentLoaded && DocumentCreationModeFlag != DocumentCreationModeFlag.Edit && DocumentCreationModeFlag != DocumentCreationModeFlag.New && PreviousDocument != null)
            {
                await oldContentWebView.EvaluateJavaScriptAsync("");

                string oldContent = null;
                if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
                {
                    oldContent = await GetBodyWithHeader(PreviousDocument.PlainTextBody, ContentType.PlainText);
                }
                else if (!string.IsNullOrWhiteSpace(PreviousDocument.HtmlBody))
                {
                    var content = PreviousDocument.HtmlBody;

                    HtmlDocument htmlDoc = null;
                    HtmlNode headNode = null;

                    try
                    {
                        htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(content);

                        foreach (var childNode1 in htmlDoc.DocumentNode.ChildNodes)
                        {
                            if (headNode != null)
                                break;

                            if (childNode1.Name == "head")
                            {
                                headNode = childNode1;
                                break;
                            }

                            foreach (var childNode2 in childNode1.ChildNodes)
                                if (childNode2.Name == "head")
                                {
                                    headNode = childNode2;
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Warning("Could not process document", ex);
                    }

                    if (htmlDoc != null && headNode != null)
                    {
                        var metaElement = htmlDoc.CreateElement("meta");
                        metaElement.SetAttributeValue("name", "viewport");
                        metaElement.SetAttributeValue("content", $"initial-scale=0.85, minimum-scale=0.5, maximum-scale=2");
                        headNode.AppendChild(metaElement);
                        content = htmlDoc.DocumentNode.OuterHtml;
                    }

                    oldContent = await GetBodyWithHeader(content, ContentType.Html);
                }
                else
                {
                    oldContent = await GetBodyWithHeader(PreviousDocument.PlainTextBody, ContentType.PlainText);
                }

                oldContentWebView.LoadHtmlString(oldContent, null);
                oldContentLoaded = true;
            }
        }

        #endregion

        #region Content processing

        string GetHtmlHeader()
        {
            var date = PreviousDocumentPreview.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsTimeAndDateString();

            var header = new StringBuilder();
            header.Append("<br/><hr/>");
            header.Append(string.Format("<b>From</b>: {0}", GetAddressTextFromPreviousDocument(DocumentAddressType.From))).Append("</br>");
            header.Append(string.Format("<b>Date</b>: {0}", date)).Append("</br>");
            header.Append(string.Format("<b>To</b>: {0}", GetAddressTextFromPreviousDocument(DocumentAddressType.To))).Append("</br>");
            var ccText = GetAddressTextFromPreviousDocument(DocumentAddressType.Cc);
            if (!string.IsNullOrWhiteSpace(ccText))
                header.Append(string.Format("<b>Cc</b>: {0}", ccText)).Append("</br>");
            header.Append(string.Format("<b>Subject</b>: {0}", PreviousDocumentPreview.Subject)).Append("</br>");
            header.Append("<br/><br/>");

            return header.ToString();
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

                var ce = htmlDocument.CreateElement("div");
                ce.ClassName = OldEditableContentClass;
                ce.Id = "editable-one";
                ce.SetAttribute("contentEditable", "true");

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

                var metaElement = parsedHeader.CreateElement("meta");
                metaElement.SetAttribute("name", "viewport");
                metaElement.SetAttribute("content", $"initial-scale=0.85, minimum-scale=0.5, maximum-scale=2");
                parsedHeader.Head.Append(metaElement);

                var ce = parsedHeader.CreateElement("div");
                ce.ClassName = OldEditableContentClass;
                ce.Id = "editable-one";
                ce.SetAttribute("contentEditable", "true");

                ce.InnerHtml = parsedHeader.Body.InnerHtml;
                parsedHeader.Body.InnerHtml = ce.OuterHtml;

                var textWriter = new StringWriter();
                parsedHeader.ToHtml(textWriter, HtmlMarkupFormatter.Instance);

                return textWriter.ToString();
            }

            return string.Empty;
        }

        async Task<(string content, string preview)> RetreiveContentAndPreview()
        {
            var htmlParser = new HtmlParser();

            var newContentString = await AsyncHelpers.InvokeOnMainThreadAsync(this,
                                                                              async () => (await newContentWebView.EvaluateJavaScriptAsync(GetWebContentJs) as NSString).ToString());
            var newContentParsed = await htmlParser.ParseAsync(newContentString);
            var newContentProcessedString = ProcessRetrievedContent(newContentParsed, NewEditableContentClass);

            await LoadOldContent();

            var oldContentString = await AsyncHelpers.InvokeOnMainThreadAsync(this,
                                                                              async () => (await oldContentWebView.EvaluateJavaScriptAsync(GetWebContentJs) as NSString).ToString());
            var oldContentParsed = await htmlParser.ParseAsync(oldContentString);
            var oldContentProcessed = ProcessRetrievedContent(oldContentParsed, OldEditableContentClass);

            if (!string.IsNullOrEmpty(oldContentProcessed))
            {
                var newContentProcessedParsed = await htmlParser.ParseAsync(newContentProcessedString);
                var oldContentProcessedParsed = await htmlParser.ParseAsync(oldContentProcessed);
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

        async Task SetWebContentPart(string parentElementClass, ContentType contentType, string content)
        {
            await newContentLoadingSemaphore.WaitAsync();

            var currentContent = await newContentWebView.EvaluateJavaScriptAsync(GetWebContentJs) as NSString;

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

            var processedContent = await ProcessInsertedContent(htmlParser, contentType, content);

            parentElement.InnerHtml = processedContent;

            var textWriter = new StringWriter();
            currentHtmlDocument.ToHtml(textWriter, HtmlMarkupFormatter.Instance);

            newContentWebView.StopLoading();
            newContentWebView.LoadHtmlString(textWriter.ToString(), null);
        }

        static async Task<string> ProcessInsertedContent(HtmlParser htmlParser, ContentType contentToInsertType, string contentToInsert)
        {
            if (contentToInsertType == ContentType.Html)
            {
                var inlinedContentToInsert = InlineStyles(contentToInsert);
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

        static string ProcessRetrievedContent(IHtmlDocument currentHtmlDocument, string elementClass)
        {
            var matchingElements = currentHtmlDocument.QuerySelectorAll("div." + elementClass);
            foreach (var matchingElement in matchingElements)
                matchingElement.Attributes.RemoveNamedItem("contenteditable");

            var processedWebContent = currentHtmlDocument.DocumentElement.OuterHtml;

            return InlineStyles(processedWebContent);
        }

        static string InlineStyles(string content)
        {
            var inlineResult = PreMailer.Net.PreMailer.MoveCssInline(content, true, null, null, true, true);
            if (inlineResult.Warnings != null && inlineResult.Warnings.Count > 0)
                CommonConfig.Logger.Warning("There were warnings when inlining CSS:\n" + string.Join("\n", inlineResult.Warnings));
            return inlineResult.Html;
        }

        static string GetPreview(IHtmlDocument contentParsed)
        {
            var textContent = contentParsed.Body.TextContent;
            return textContent.SafeSubstring(0, 300).Trim();
        }

        #endregion

        #region IUIGestureRecognizerDelegate

        [Export("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
        public bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
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

        [Export("userContentController:didReceiveScriptMessage:")]
        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            var responseDict = message.Body as NSDictionary;
            if (responseDict == null)
                return;

            var justLoadedNumber = responseDict["justLoaded"] as NSNumber;
            var resizedNumber = responseDict["resized"] as NSNumber;
            var mutatedNumber = responseDict["mutated"] as NSNumber;
            var domLoadedNumber = responseDict["domLoaded"] as NSNumber;
            var keypressedNumber = responseDict["keypressed"] as NSNumber;

            var justLoaded = justLoadedNumber != null && justLoadedNumber.BoolValue;
            var resized = resizedNumber != null && resizedNumber.BoolValue;
            var mutated = mutatedNumber != null && mutatedNumber.BoolValue;
            var domLoaded = domLoadedNumber != null && domLoadedNumber.BoolValue;
            var keyPressed = keypressedNumber != null && keypressedNumber.BoolValue;

            Action<WKWebView, NSLayoutConstraint> resizeAction = null;
            resizeAction = (wv, nslc) => DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(150)), () =>
            {
                if (wv.IsLoading)
                {
                    resizeAction(wv, nslc);
                }
                else if (Math.Abs(nslc.Constant - wv.ScrollView.ContentSize.Height) > 10) //Condition to avoid loop on size increase
                {
                    nslc.Constant = wv.ScrollView.ContentSize.Height;
                    SetNeedsLayout();
                }
            });

            Action<WKWebView, NSLayoutConstraint> stopLoadingAction = null;
            stopLoadingAction = (wv, nslc) => DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(3500)), () =>
            {
                if (wv.IsLoading)
                {
                    wv.StopLoading();
                    resizeAction(wv, nslc);
                }
            });
            if (keyPressed)
            {
                ScrollForEnterPressed();
            }
            if (domLoaded)
            {
                if (userContentController == newContentWebView.Configuration.UserContentController)
                {
                    stopLoadingAction(newContentWebView, newContentHeightConstraint);
                }
                else if (userContentController == oldContentWebView.Configuration.UserContentController)
                {
                    stopLoadingAction(oldContentWebView, oldContentHeightConstraint);
                }
            }
            if (justLoaded || resized || mutated)
            {
                if (userContentController == newContentWebView.Configuration.UserContentController)
                {
                    if (resized && newContentResized)
                        return;

                    newContentResized = resized;
                    resizeAction(newContentWebView, newContentHeightConstraint);
                }
                else if (userContentController == oldContentWebView.Configuration.UserContentController)
                {
                    if (resized && oldContentResized)
                        return;

                    oldContentResized = resized;
                    resizeAction(oldContentWebView, oldContentHeightConstraint);
                }
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
                    await webView.EvaluateJavaScriptAsync("");
                    DidFinishNavigationAction?.Invoke();
                });
            }
        }
    }
}