//
// Project: Mark5.Mobile.IOS
// File: TextView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public abstract class TextSubView : DocumentView
    {
        UILabel label;
        protected UITextView TextView;

        protected TextSubView(string labelText)
        {
            Initialize(labelText);
        }

        void Initialize(string labelText)
        {
            label = new UILabel();
            label.Text = labelText + ":";
            label.Font = Theme.DefaultFont;
            label.TextColor = UIColor.LightGray;
            label.Opaque = false;
            label.TranslatesAutoresizingMaskIntoConstraints = false;
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            AddSubview(label);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                });

            TextView = new UITextView();
            TextView.Font = Theme.DefaultFont;
            TextView.Editable = false;
            TextView.Opaque = false;
            TextView.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            TextView.AutocorrectionType = UITextAutocorrectionType.Yes;
            TextView.SpellCheckingType = UITextSpellCheckingType.Yes;
            TextView.TextContainer.LineFragmentPadding = 0.0f;
            TextView.TextContainerInset = UIEdgeInsets.Zero;
            TextView.ClipsToBounds = false;
            TextView.ScrollEnabled = false;
            TextView.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(TextView);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1.0f, InnerMargin),
                    NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                    NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin),
                });
        }

    }
}
