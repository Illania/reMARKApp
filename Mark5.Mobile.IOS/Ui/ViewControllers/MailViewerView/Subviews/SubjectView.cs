//
// Project: Mark5.Mobile.IOS
// File: SubjectView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    
    public class SubjectView : MailViewerSubview
    {
    
        readonly UITextView textView;

        public SubjectView()
        {
            textView = new UITextView();
            textView.Font = Theme.DefaultFont.WithRelativeSize(4f);
            textView.Editable = false;
            textView.Opaque = false;
            textView.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            textView.AutocorrectionType = UITextAutocorrectionType.Yes;
            textView.SpellCheckingType = UITextSpellCheckingType.Yes;
            textView.TextContainer.LineFragmentPadding = 0f;
            textView.TextContainerInset = UIEdgeInsets.Zero;
            textView.ClipsToBounds = false;
            textView.ScrollEnabled = false;
            textView.TranslatesAutoresizingMaskIntoConstraints = false;
            ContainerView.AddSubview(textView);
            ContainerView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
                });
        }

        public override void RefreshView()
        {
            if (MailMessage != null)
                textView.Text = MailMessage.Subject;
        }

        public override void UpdateVisibility()
        {
            if (MailMessage == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(MailMessage.Subject);
        }
    }
}
