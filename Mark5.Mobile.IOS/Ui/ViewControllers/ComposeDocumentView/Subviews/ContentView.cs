//
// Project: Mark5.Mobile.IOS
// File: ContentView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class ContentView : ComposeDocumentView
    {
        public event EventHandler Edited = delegate { };

        UITextView editTextView;
        UITextView previousTextView;
        UIButton expandButton;

        DocumentCreationModeFlag creationModeFlag;

        NSTextContainer editContentContainer;
        NSLayoutManager editContentLayoutManager;
        NSTextStorage editContentStorage;
        NSTextContainer previousContentContainer;
        NSLayoutManager previousContentLayoutManager;
        NSTextStorage previousContentStorage;

        bool previousDocumentContentLoaded;

        Dictionary<UIView, NSLayoutConstraint[]> constraintsStash;
        NSLayoutConstraint zeroHeightConstraint;
        NSLayoutConstraint minimumHeightConstraint;

        public ContentView()
        {
            MinimumHeight = 200.0f;
            constraintsStash = new Dictionary<UIView, NSLayoutConstraint[]>();

            Initialize();
        }

        void Initialize()
        {
            editContentContainer = new NSTextContainer();

            editContentLayoutManager = new NSLayoutManager();
            editContentLayoutManager.AddTextContainer(editContentContainer);

            editContentStorage = new NSTextStorage();
            editContentStorage.AddLayoutManager(editContentLayoutManager);

            editTextView = new UITextView(CGRect.Empty, editContentContainer);
            editTextView.Font = Theme.DefaultDocumentFont;
            editTextView.Opaque = false;
            editTextView.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            editTextView.AutocorrectionType = UITextAutocorrectionType.Yes;
            editTextView.SpellCheckingType = UITextSpellCheckingType.Yes;
            editTextView.DataDetectorTypes = UIDataDetectorType.All;
            editTextView.ScrollEnabled = false;
            editTextView.ClipsToBounds = false;
            editTextView.TranslatesAutoresizingMaskIntoConstraints = false;
            editTextView.Changed += (sender, e) => Edited(this, EventArgs.Empty);
            editTextView.SelectionChanged += HandleTextViewSelectionChanged;
            editTextView.Started += HandleScrollToView;
            AddSubview(editTextView);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(editTextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(editTextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                    NSLayoutConstraint.Create(editTextView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                    NSLayoutConstraint.Create(editTextView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, 200),
                });
        }

        void InitializePreviousContentControls()
        {
            expandButton = UIButton.FromType(UIButtonType.System);
            expandButton.SetTitle(Localization.GetString("show_original_message"), UIControlState.Normal);
            expandButton.TranslatesAutoresizingMaskIntoConstraints = false;
            expandButton.Font = Theme.DefaultFont;
            expandButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
            expandButton.Opaque = false;
            expandButton.TouchUpInside += HandleExpandButtonTapped;

            AddSubview(expandButton);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, editTextView, NSLayoutAttribute.Bottom, 1.0f, 0),
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, 2*HorizontalMargin),
                NSLayoutConstraint.Create(expandButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -2*HorizontalMargin),
            });

            previousContentContainer = new NSTextContainer();

            previousContentLayoutManager = new NSLayoutManager();
            previousContentLayoutManager.AddTextContainer(previousContentContainer);

            previousContentStorage = new NSTextStorage();
            previousContentStorage.AddLayoutManager(previousContentLayoutManager);

            previousTextView = new UITextView(CGRect.Empty, previousContentContainer);
            previousTextView.Hidden = true;
            previousTextView.Layer.BorderWidth = 0.7f;
            previousTextView.Layer.BorderColor = new UITableView().SeparatorColor.CGColor;
            previousTextView.Font = Theme.DefaultDocumentFont;
            previousTextView.Opaque = false;
            previousTextView.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            previousTextView.AutocorrectionType = UITextAutocorrectionType.Yes;
            previousTextView.SpellCheckingType = UITextSpellCheckingType.Yes;
            previousTextView.DataDetectorTypes = UIDataDetectorType.All;
            previousTextView.ScrollEnabled = false;
            previousTextView.ClipsToBounds = false;
            previousTextView.TranslatesAutoresizingMaskIntoConstraints = false;
            previousTextView.Changed += (sender, e) => Edited(this, EventArgs.Empty);
            previousTextView.SelectionChanged += HandleTextViewSelectionChanged;
            previousTextView.Started += HandleScrollToView;
            AddSubview(previousTextView);

            minimumHeightConstraint = NSLayoutConstraint.Create(previousTextView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, 0.5f);
            zeroHeightConstraint = NSLayoutConstraint.Create(previousTextView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, 0.0f);

            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(previousTextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, expandButton, NSLayoutAttribute.Bottom, 1.0f, 0),
                NSLayoutConstraint.Create(previousTextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                NSLayoutConstraint.Create(previousTextView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                NSLayoutConstraint.Create(previousTextView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin),
                zeroHeightConstraint,
            });
        }

        #region Overrides

        public override  Task RefreshView()
        {
            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                var result = GetContentAndTypeFromDocument();
                var content = result.Item1;
                var nsDocumentType = result.Item2;
                var contentAttributedString = content.ToNSAttributedString(nsDocumentType, Theme.DefaultDocumentFont);
                if (contentAttributedString != null && contentAttributedString.Length > 0)
                {
                    editTextView.AttributedText = contentAttributedString;
                }
            }
            else
            {
                expandButton.Hidden &= PreviousDocument == null;
            }

            return Task.CompletedTask;
        }

        public override  Task UpdateDocument()
        {
            Document.HtmlBody = GetHtmlText();
            //TODO need to update also the document preview preview, but we need anglesharp for that
            return Task.CompletedTask;
        }

        #endregion

        #region Event handlers

        void HandleTextViewSelectionChanged(object sender, EventArgs e)
        {
            var textView = sender as UITextView;
            var parentScrollView = Superview as UIScrollView;
            if (parentScrollView != null && textView.SelectedTextRange != null)
            {
                var cursorRect = ConvertRectToView(textView.GetCaretRectForPosition(textView.SelectedTextRange.Start), parentScrollView);
                var bottomOfVisibleArea = parentScrollView.ContentOffset.Y + parentScrollView.Frame.Bottom - parentScrollView.ContentInset.Bottom;
                if (!nfloat.IsInfinity(cursorRect.Bottom) && cursorRect.Bottom > bottomOfVisibleArea)
                {
                    var newVerticalOffset = parentScrollView.ContentOffset.Y + cursorRect.Bottom - bottomOfVisibleArea + textView.Font.LineHeight;
                    if (!nfloat.IsInfinity(newVerticalOffset))
                    {
                        parentScrollView.SetContentOffset(new CGPoint(parentScrollView.ContentOffset.X, newVerticalOffset), true);
                    }
                }
            }
        }

        void HandleExpandButtonTapped(object sender, EventArgs e)
        {
            if (previousTextView.Hidden)
            {
                expandButton.SetTitle(Localization.GetString("hide_original_message"), UIControlState.Normal);
                previousTextView.Hidden = false;

                LoadPreviousDocumentContent();

                RemoveConstraint(zeroHeightConstraint);
                AddConstraint(minimumHeightConstraint);

                previousTextView.RestoreConstaints(constraintsStash);

                constraintsStash.Clear();

                Animate(0.15d, LayoutIfNeeded);
            }
            else
            {
                expandButton.SetTitle(Localization.GetString("show_original_message"), UIControlState.Normal);
                previousTextView.Hidden = true;

                constraintsStash = previousTextView.BackupConstaints();

                RemoveConstraint(minimumHeightConstraint);
                AddConstraint(zeroHeightConstraint);

                Animate(0.15d, LayoutIfNeeded);
            }
        }

        void LoadPreviousDocumentContent()
        {
            if (PreviousDocument == null || previousDocumentContentLoaded || CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                return;
            }

            var result = GetContentAndTypeFromDocument(); //TODO need either to pass the document or to change name
            var content = result.Item1;
            var nsDocumentType = result.Item2;

            var attributedContent = content.ToNSAttributedString(nsDocumentType, Theme.DefaultDocumentFont);
            var attributedHeader = GetAttributedHeader();

            var mutableAttributedString = new NSMutableAttributedString();

            if (attributedHeader != null && attributedHeader.Length > 0)
            {
                mutableAttributedString.Append(attributedHeader);
            }

            if (attributedContent != null && attributedContent.Length > 0)
            {
                mutableAttributedString.Append(attributedContent);
            }

            previousTextView.TextStorage.Insert(mutableAttributedString, 0);
            previousDocumentContentLoaded = true;
        }

        #endregion

        #region Public methods

        public string GetHtmlText()
        {
            return RetrieveCombinedText().ToHTMLString(Theme.DefaultOutgoingDocumentFont);
        }

        public string GetPlainText()
        {
            return RetrieveCombinedText().Value;
        }

        public void SetCreationModeFlag(DocumentCreationModeFlag modeFlag)
        {
            if (creationModeFlag != DocumentCreationModeFlag.None)
            {
                throw new InvalidOperationException("The creation mode has already been set");
            }

            creationModeFlag = modeFlag;
            if (creationModeFlag == DocumentCreationModeFlag.New || creationModeFlag == DocumentCreationModeFlag.Edit)
            {
                //If not added there are problems with the view not expanding to accomodate longer emails
                AddConstraint(NSLayoutConstraint.Create(editTextView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin));
            }
            else
            {
                InitializePreviousContentControls();
            }
        }

        public void InsertTemplate(Template template)
        {
            InsertTemplateAt(template.Content, template.ContentType);
        }

        public void InsertTemplateAt(string templateString, ContentType contentType, int location = 0)
        {
            var templateAttributedContent = templateString.ToNSAttributedString(contentType.ToNSDocumentType(), Theme.DefaultDocumentFont);
            if (templateAttributedContent != null && templateAttributedContent.Length > 0)
            {
                editTextView.TextStorage.Insert(templateAttributedContent, location);
            }

            editTextView.SelectedRange = new NSRange(location, 0);
        }

        #endregion

        #region Utilities

        Tuple<string, NSDocumentType> GetContentAndTypeFromDocument()
        {
            NSDocumentType nsDocumentType = NSDocumentType.Unknown;
            string content;
            if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
            {
                nsDocumentType = NSDocumentType.PlainText;
                content = PreviousDocument.PlainTextBody;
            }

            if (PlatformConfig.Preferences.DocumentBodyRequestType == DocumentBodyTypeRequest.PlainTextOnly)
            {
                nsDocumentType = NSDocumentType.PlainText;
                content = PreviousDocument.PlainTextBody;
            }
            else if (!string.IsNullOrWhiteSpace(PreviousDocument.HtmlBody))
            {
                nsDocumentType = NSDocumentType.HTML;
                content = PreviousDocument.HtmlBody;
            }
            else
            {
                nsDocumentType = NSDocumentType.PlainText;
                content = PreviousDocument.PlainTextBody;
            }

            return new Tuple<string, NSDocumentType>(content, nsDocumentType);
        }

        NSAttributedString RetrieveCombinedText()
        {
            if (creationModeFlag == DocumentCreationModeFlag.Edit || creationModeFlag == DocumentCreationModeFlag.New)
            {
                return editTextView.AttributedText;
            }

            LoadPreviousDocumentContent();

            var mutableAttributedString = new NSMutableAttributedString();
            mutableAttributedString.Append(editTextView.AttributedText);
            mutableAttributedString.Append(previousTextView.AttributedText);

            return mutableAttributedString;
        }

        NSAttributedString GetAttributedHeader()
        {
            var header = new StringBuilder();

            var fromText = GetAddressText(PreviousDocumentPreview, DocumentAddressType.From);
            var toText = GetAddressText(PreviousDocumentPreview, DocumentAddressType.To);
            var ccText = GetAddressText(PreviousDocumentPreview, DocumentAddressType.Cc);
            var subject = PreviousDocumentPreview.Subject;

            header.Append("<br/>");
            header.Append(string.Format("<b>From</b>: {0}", WebUtility.HtmlEncode(fromText))).Append("</br>");
            header.Append(string.Format("<b>Date</b>: {0}", PreviousDocumentPreview.DateReceivedTimestamp)).Append("</br>"); //TODO need to fix the dates
            header.Append(string.Format("<b>To</b>: {0}", WebUtility.HtmlEncode(toText))).Append("</br>");
            if (!string.IsNullOrWhiteSpace(ccText))
            {
                header.Append(string.Format("<b>Cc</b>: {0}", WebUtility.HtmlEncode(ccText))).Append("</br>");
            }
            header.Append(string.Format("<b>Subject</b>: {0}", WebUtility.HtmlEncode(subject))).Append("</br>");
            header.Append("<br/>");

            var attributedHeader = header.ToString().ToNSAttributedString(NSDocumentType.HTML, Theme.DefaultDocumentFont);

            return attributedHeader;
        }

        string GetAddressText(DocumentPreview document, DocumentAddressType addressType)
        {
            var sb = new StringBuilder();
            var addresses = document.Addresses.Where(da => da.AddressType == addressType).ToList();
            for (int i = 0; i < addresses.Count; i++)
            {
                var hasName = !string.IsNullOrWhiteSpace(addresses[i].Name);
                if (hasName)
                {
                    sb.Append(addresses[i].Name).Append(" <");
                }
                sb.Append(addresses[i].Address);
                if (hasName)
                {
                    sb.Append(">");
                }
                if (i < addresses.Count - 1)
                {
                    sb.Append(", ");
                }

            }
            return sb.ToString();
        }

        #endregion
    }
}
