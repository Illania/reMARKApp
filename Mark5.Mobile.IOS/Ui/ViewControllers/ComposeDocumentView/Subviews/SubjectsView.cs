//
// Project: Mark5.Mobile.IOS
// File: SubjectsView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView.Subviews
{
    public class SubjectsView : ComposeDocumentSubview
    {
        public event EventHandler Edited = delegate { };

        UILabel label;
        UITextView textView;

        public SubjectsView()
        {
            Initialize();
        }

        void Initialize()
        {
            label = new UILabel();
            label.Text = Localization.GetString("subject");
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

            textView = new UITextView();
            textView.Font = Theme.DefaultFont;
            textView.Opaque = false;
            textView.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            textView.AutocorrectionType = UITextAutocorrectionType.Yes;
            textView.SpellCheckingType = UITextSpellCheckingType.Yes;
            textView.TextContainer.LineFragmentPadding = 0.0f;
            textView.TextContainerInset = UIEdgeInsets.Zero;
            textView.ClipsToBounds = false;
            textView.ScrollEnabled = false;
            textView.KeyboardType = UIKeyboardType.Default;
            textView.TranslatesAutoresizingMaskIntoConstraints = false;
            textView.Started += HandleScrollToView;
            textView.Changed += (sender, e) => Edited(this, EventArgs.Empty);
            AddSubview(textView);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1.0f, InnerMargin),
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                    NSLayoutConstraint.Create(textView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin),
                });
        }

        //TODO remember about all the unsubscription from view

        #region Overrides

        public override Task RefreshView()
        {
            if (CreationModeFlag == DocumentCreationModeFlag.None || CreationModeFlag == DocumentCreationModeFlag.New)
            {
                return Task.CompletedTask;
            }

            switch (CreationModeFlag)
            {
                case DocumentCreationModeFlag.Edit:
                    textView.Text = PreviousDocumentPreview.Subject;
                    break;
                case DocumentCreationModeFlag.Reply:
                case DocumentCreationModeFlag.ReplyAll:
                    textView.Text = $"Re: {PreviousDocumentPreview.Subject}";
                    break;
                case DocumentCreationModeFlag.Forward:
                    textView.Text = $"Fw: {PreviousDocumentPreview.Subject}";
                    break;
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            DocumentPreview.Subject = textView.Text;
            return Task.CompletedTask;
        }

        #endregion

    }
}
