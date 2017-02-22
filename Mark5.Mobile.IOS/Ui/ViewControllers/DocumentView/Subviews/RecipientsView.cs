//
// Project: Mark5.Mobile.IOS
// File: RecipientsView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Linq;
using System.Text.RegularExpressions;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class RecipientsView : DocumentSubView
    {
        const string EmailSeparator = ", ";
        const string RecipentRegex = @"[^,]*";

        readonly DocumentAddressType addressType;

        UILabel titleLabel;
        UITextView textView;
        UITapGestureRecognizer textViewTapGestureRecognizer;

        bool expanded;

        public event EventHandler<RecipentTappedEventArgs> RecipentTapped = delegate { };

        public RecipientsView(DocumentAddressType addressType)
        {
            this.addressType = addressType;
            Initialize();
        }

        void Initialize()
        {
            titleLabel = new UILabel();
            titleLabel.Text = GetTitle() + ":";
            titleLabel.Font = Theme.DefaultFont;
            titleLabel.TextColor = UIColor.LightGray;
            titleLabel.Opaque = false;
            titleLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            titleLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            titleLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            titleLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(titleLabel);
            ContainerView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1.0f, HorizontalMargin)
                });

            var textStorage = new NSTextStorage();
            textStorage.AddAttribute(UIStringAttributeKey.Font, Theme.DefaultFont, new NSRange(0, 0));
            var layoutManager = new NSLayoutManager();
            textStorage.AddLayoutManager(layoutManager);
            var textContainer = new NSTextContainer();
            layoutManager.AddTextContainer(textContainer);

            textView = new UITextView(CGRect.Empty, textContainer);
            textView.Font = Theme.DefaultFont;
            textView.Opaque = false;
            textView.TextContainer.LineFragmentPadding = 0.0f;
            textView.TextContainerInset = UIEdgeInsets.Zero;
            textView.ClipsToBounds = false;
            textView.ScrollEnabled = false;
            textView.TranslatesAutoresizingMaskIntoConstraints = false;
            textView.TextContainer.MaximumNumberOfLines = 1;
            textView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            ContainerView.AddSubview(textView);
            ContainerView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, titleLabel, NSLayoutAttribute.Right, 1.0f, InnerMargin),
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin),
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin)
                });

            textViewTapGestureRecognizer = new UITapGestureRecognizer();
            textViewTapGestureRecognizer.AddTarget(HandleTextTapped);
            textViewTapGestureRecognizer.NumberOfTapsRequired = 1;
            textView.AddGestureRecognizer(textViewTapGestureRecognizer);
        }

        #region DocumentSubView overrides

        public override void RefreshView()
        {
            if (DocumentPreview != null)
            {
                Func<DocumentAddress, string> addressText = (da) =>
                {
                    if (!string.IsNullOrWhiteSpace(da.Name) && string.IsNullOrWhiteSpace(da.Address))
                    {
                        return da.Name;
                    }
                    if (!string.IsNullOrWhiteSpace(da.Name) && !string.IsNullOrWhiteSpace(da.Address))
                    {
                        return da.Name + " <" + da.Address + ">";
                    }
                    if (string.IsNullOrWhiteSpace(da.Name) && !string.IsNullOrWhiteSpace(da.Address))
                    {
                        return da.Address;
                    }

                    return string.Empty;
                };

                var prettyAddresses = DocumentPreview.Addresses.Where(da => da.AddressType == addressType).Select(addressText);
                var text = string.Join(EmailSeparator, prettyAddresses);

                textView.TextStorage.BeginEditing();
                textView.TextStorage.SetString(text.ToNSAttributedString());
                textView.TextStorage.EndEditing();

                CorrectMarkup();
            }
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = !DocumentPreview.Addresses.Any(da => da.AddressType == addressType);
        }

        #endregion

        #region Event handlers and delegate

        void HandleTextTapped()
        {
            if (!expanded && textView.IsTruncated())
            {
                ExpandView();
                return;
            }

            var tapPosition = textView.GetClosestPositionToPoint(textViewTapGestureRecognizer.LocationInView(textView));
            var offset = textView.GetOffsetFromPosition(textView.BeginningOfDocument, tapPosition);

            var beforeSubstring = textView.Text.SafeSubstring(0, (int)offset).SafeSubstringAfterLast(EmailSeparator, StringComparison.CurrentCultureIgnoreCase).Trim();
            var afterSubstring = textView.Text.SafeSubstring((int)offset).SafeSubstringBefore(EmailSeparator, StringComparison.CurrentCultureIgnoreCase).Trim();

            var tappedRecipent = beforeSubstring + afterSubstring;

            CommonConfig.Logger.Trace(string.Format("Tapped recipent. [recipent={0}]", tappedRecipent));

            RecipentTapped(this, new RecipentTappedEventArgs(tappedRecipent));
        }

        #endregion

        #region Helper methods

        string GetTitle()
        {
            switch (addressType)
            {
                case DocumentAddressType.To:
                    return Localization.GetString("to");
                case DocumentAddressType.Cc:
                    return Localization.GetString("cc");
                case DocumentAddressType.Bcc:
                    return Localization.GetString("bcc");
                case DocumentAddressType.From:
                    return Localization.GetString("from");
                default:
                    throw new ArgumentException(string.Format("Unknown type. [addressType={0}]", addressType));
            }
        }

        void CorrectMarkup()
        {
            textView.TextStorage.BeginEditing();

            textView.TextStorage.AddAttribute(UIStringAttributeKey.Font, Theme.DefaultFont, new NSRange(0, textView.Text.Length));
            textView.TextStorage.RemoveAttribute(UIStringAttributeKey.ForegroundColor, new NSRange(0, textView.Text.Length));

            var matches = Regex.Matches(textView.Text, RecipentRegex, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var textInMatch = textView.Text.SafeSubstring(match.Index, match.Length);
                if (Validator.ContainsValidEmails(textInMatch))
                {
                    textView.TextStorage.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.TintColor, new NSRange(match.Index, match.Length));
                }
            }

            textView.TextStorage.EndEditing();
        }

        void ExpandView()
        {
            if (expanded)
            {
                return;
            }

            // Work around to force text view layout
            textView.TextStorage.BeginEditing();
            textView.TextStorage.Insert(" ".ToNSAttributedString(), 0);
            textView.TextStorage.DeleteRange(new NSRange(0, 1));
            textView.TextStorage.EndEditing();

            Animate(0.2d, () =>
                {
                    textView.TextContainer.MaximumNumberOfLines = 0;
                    textView.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;

                    Superview.SetNeedsLayout();
                    Superview.LayoutIfNeeded();

                    expanded = true;
                });
        }

        #endregion

    }

    public class RecipentTappedEventArgs : EventArgs
    {

        public string Recipent
        {
            get;
            private set;
        }

        public RecipentTappedEventArgs(string recipent)
        {
            Recipent = recipent;
        }
    }
}
