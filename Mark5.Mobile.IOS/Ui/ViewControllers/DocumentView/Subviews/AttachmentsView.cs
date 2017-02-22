//
// Project: Mark5.Mobile.IOS
// File: AttachmentsView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class AttachmentsView : DocumentSubView
    {
        UILabel titleLabel;
        UIStackView stackView;

        public event EventHandler<AttachmentButtonTappedEventArgs> AttachmentTapped = delegate { };

        public AttachmentsView()
        {
            Initialize();
        }

        void Initialize()
        {
            titleLabel = new UILabel
            {
                Text = Localization.GetString("attachments") + ":",
                Font = Theme.DefaultFont,
                TextColor = UIColor.LightGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContainerView.AddSubview(titleLabel);
            ContainerView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1.0f, HorizontalMargin)
                });

            stackView = new UIStackView
            {
                Opaque = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = InnerMargin,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContainerView.AddSubview(stackView);
            ContainerView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, titleLabel, NSLayoutAttribute.Bottom, 1.0f, InnerMargin),
                    NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                    NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                    NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin)
                });
        }

        public override void RefreshView()
        {
            if (Document == null)
            {
                return;
            }

            stackView.ArrangedSubviews.ForEach(v =>
                {
                    stackView.RemoveArrangedSubview(v);
                    v.RemoveFromSuperview();
                });

            foreach (var ad in Document.Attachments)
            {
                var alssv = new AttachmentsSubView(this, ad);
                stackView.AddArrangedSubview(alssv);
            }

            if (Container != null)
            {
                foreach (var ad in Container.LocalAttachments)
                {
                    var alssv = new AttachmentsSubView(this, ad);
                    stackView.AddArrangedSubview(alssv);
                }
            }
        }

        public override void UpdateVisibility()
        {
            if (Document == null)
            {
                Hidden = true;
                return;
            }

            Hidden = Document.Attachments.Count < 1 && Container?.LocalAttachments.Count < 1;
        }

        #region Event handlers

        public void HandleAttachmentButtonTapped(AttachmentButtonTappedEventArgs eventArgs)
        {
            AttachmentTapped(this, eventArgs);
        }

        #endregion
    }

    class AttachmentsSubView : UIView
    {

        public IAttachmentDescription Attachment
        {
            get
            {
                return attachmentDescription;
            }
        }

        readonly AttachmentsView view;
        readonly IAttachmentDescription attachmentDescription;

        UIButton attachmentButton;

        public AttachmentsSubView(AttachmentsView view, IAttachmentDescription attachmentDescription)
        {
            this.view = view;
            this.attachmentDescription = attachmentDescription;

            InitSubViews();
        }

        void InitSubViews()
        {
            Opaque = false;
            TranslatesAutoresizingMaskIntoConstraints = false;

            attachmentButton = UIButton.FromType(UIButtonType.RoundedRect);
            attachmentButton.TitleLabel.Font = Theme.DefaultFont;
            attachmentButton.SetTitle(attachmentDescription.Name + " (" + UI.PrettyFileSize(attachmentDescription.SizeInBytes) + ")", UIControlState.Normal);
            attachmentButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            attachmentButton.TouchUpInside += (sender, e) => view.HandleAttachmentButtonTapped(new AttachmentButtonTappedEventArgs(attachmentDescription));
            attachmentButton.Opaque = false;
            attachmentButton.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(attachmentButton);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(attachmentButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(attachmentButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(attachmentButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(attachmentButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, 0.0f)
                });
        }
    }

    public class AttachmentButtonTappedEventArgs : EventArgs
    {

        public IAttachmentDescription Attachment
        {
            get;
            private set;
        }

        public AttachmentButtonTappedEventArgs(IAttachmentDescription attachment)
        {
            Attachment = attachment;
        }
    }
}
