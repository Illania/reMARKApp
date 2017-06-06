//
// Project: Mark5.Mobile.IOS
// File: RecipientsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class RecipientsView : MailViewerSubview
    {
        public enum Type
        {
            To,
            Cc,
            Bcc,
            From,
            ReplyTo
        }

        readonly Type type;
        readonly UITextView textView;

        bool expanded;

        public RecipientsView(Type type)
        {
            this.type = type;

            var titleLabel = new UILabel();
            titleLabel.Text = GetTitle() + ":";
            titleLabel.Font = Theme.DefaultFont;
            titleLabel.TextColor = UIColor.LightGray;
            titleLabel.Opaque = false;
            titleLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            titleLabel.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            titleLabel.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            titleLabel.SetContentCompressionResistancePriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(titleLabel);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
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
            textView.TextContainer.LineFragmentPadding = 0f;
            textView.TextContainerInset = UIEdgeInsets.Zero;
            textView.ClipsToBounds = false;
            textView.ScrollEnabled = false;
            textView.TranslatesAutoresizingMaskIntoConstraints = false;
            textView.TextContainer.MaximumNumberOfLines = 1;
            textView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            ContainerView.AddSubview(textView);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, titleLabel, NSLayoutAttribute.Right, 1f, InnerMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin)
            });

            var textViewTapGestureRecognizer = new UITapGestureRecognizer();
            textViewTapGestureRecognizer.AddTarget(HandleTextTapped);
            textViewTapGestureRecognizer.NumberOfTapsRequired = 1;
            textView.AddGestureRecognizer(textViewTapGestureRecognizer);
        }

        public override void RefreshView()
        {
            if (MailMessage != null)
            {
                textView.TextStorage.BeginEditing();
                textView.TextStorage.SetString(GetValue().ToNSAttributedString());
                textView.TextStorage.AddAttribute(UIStringAttributeKey.Font, Theme.DefaultFont, new NSRange(0, textView.Text.Length));
                textView.TextStorage.EndEditing();
            }
        }

        public override void UpdateVisibility()
        {
            if (MailMessage == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(GetValue());
        }

        void HandleTextTapped()
        {
            if (!expanded && textView.IsTruncated())
            {
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
        }

        string GetTitle()
        {
            switch (type)
            {
                case Type.To:
                    return Localization.GetString("to");
                case Type.Cc:
                    return Localization.GetString("cc");
                case Type.Bcc:
                    return Localization.GetString("bcc");
                case Type.From:
                    return Localization.GetString("from");
                case Type.ReplyTo:
                    return Localization.GetString("reply_to");
                default:
                    throw new ArgumentException(string.Format("Unknown type. [addressType={0}]", type));
            }
        }

        string GetValue()
        {
            switch (type)
            {
                case Type.To:
                    return MailMessage?.To?.AsString;
                case Type.Cc:
                    return MailMessage?.Cc?.AsString;
                case Type.Bcc:
                    return MailMessage?.Bcc?.AsString;
                case Type.From:
                    return MailMessage?.From?.AsString;
                case Type.ReplyTo:
                    return MailMessage?.ReplyTo?.AsString;
                default:
                    throw new ArgumentException(string.Format("Unknown type. [addressType={0}]", type));
            }
        }
    }
}