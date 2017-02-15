//
// Project: Mark5.Mobile.IOS
// File: AttachmentsView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class AttachmentsView : ComposeDocumentSubView
    {
        UILabel titleLabel;
        UIStackView stackView;

        List<IAttachmentDescription> attachmentsDescription = new List<IAttachmentDescription>();

        public event EventHandler<IAttachmentDescription> AttachmentClicked = delegate { };
        public event EventHandler<IAttachmentDescription> DeleteAttachmentClicked = delegate { };

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
            AddSubview(titleLabel);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin)
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
            AddSubview(stackView);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, titleLabel, NSLayoutAttribute.Bottom, 1.0f, InnerMargin),
                    NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                    NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                    NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin)
                });

            Hidden = true;
        }

        #region Public methods

        public override Task RefreshView()
        {
            attachmentsDescription.Clear();
            stackView.Subviews.OfType<AttachmentsSubView>().ForEach(v => v.RemoveFromSuperview());

            if (CreationModeFlag == DocumentCreationModeFlag.Forward || CreationModeFlag == DocumentCreationModeFlag.Edit) //TODO Any counter example to this?
            {
                foreach (var attachmentDescription in PreviousDocument.Attachments)
                {
                    AddAttachment(attachmentDescription);
                }
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            var remoteAttachments = attachmentsDescription.OfType<AttachmentDescription>().ToList();
            Document.Attachments.AddRange(remoteAttachments);
            DocumentPreview.AttachmentsCount = attachmentsDescription.Count;

            //Nothing to do for local attachments, they're saved on disk

            return Task.CompletedTask;
        }

        public void AddAttachment(IAttachmentDescription attachment)
        {
            attachmentsDescription.Add(attachment);

            var alssv = new AttachmentsSubView(attachment, HandleAttachmentClicked, HandleDeleteAttachmentClicked);
            stackView.AddArrangedSubview(alssv);

            UpdateVisibility();
        }

        public void RemoveAttachment(object view, IAttachmentDescription attachmentDescrption)
        {
            attachmentsDescription.Remove(attachmentDescrption);
            (view as UIView).RemoveFromSuperview();

            UpdateVisibility();
        }

        public List<OutgoingDocumentAttachmentDescription> GetOutgoingAttachments()
        {
            return attachmentsDescription.OfType<OutgoingDocumentAttachmentDescription>().ToList();
        }

        #endregion

        #region Utilities

        void UpdateVisibility()
        {
            Hidden = attachmentsDescription.Count < 1;
        }

        void HandleAttachmentClicked(AttachmentsSubView view, IAttachmentDescription attachmentDescrption)
        {
            AttachmentClicked(view, attachmentDescrption);
        }

        void HandleDeleteAttachmentClicked(AttachmentsSubView view, IAttachmentDescription attachmentDescrption)
        {
            DeleteAttachmentClicked(view, attachmentDescrption);
        }

        #endregion

        class AttachmentsSubView : UIStackView
        {
            readonly IAttachmentDescription attachmentDescription;
            readonly Action<AttachmentsSubView, IAttachmentDescription> attachmentClickedAction;
            readonly Action<AttachmentsSubView, IAttachmentDescription> deleteAttachmentClickedAction;

            UIButton filenameButton;
            UIButton deleteButton;

            public AttachmentsSubView(IAttachmentDescription attachmentDescription, Action<AttachmentsSubView, IAttachmentDescription> attachmentClickedAction,
                                      Action<AttachmentsSubView, IAttachmentDescription> deleteAttachmentClickedAction)
            {
                this.attachmentDescription = attachmentDescription;
                this.attachmentClickedAction = attachmentClickedAction;
                this.deleteAttachmentClickedAction = deleteAttachmentClickedAction;

                InitView();
            }

            void InitView()
            {
                Opaque = false;
                TranslatesAutoresizingMaskIntoConstraints = false;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Axis = UILayoutConstraintAxis.Horizontal;

                filenameButton = UIButton.FromType(UIButtonType.System);
                filenameButton.TranslatesAutoresizingMaskIntoConstraints = false;
                filenameButton.TitleLabel.Font = Theme.DefaultFont;
                filenameButton.SetTitle(attachmentDescription.Name + " (" + UI.PrettyFileSize(attachmentDescription.SizeInBytes) + ")", UIControlState.Normal);
                filenameButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
                filenameButton.Opaque = false;
                filenameButton.TouchUpInside += (sender, e) => attachmentClickedAction(this, attachmentDescription);
                AddArrangedSubview(filenameButton);

                deleteButton = UIButton.FromType(UIButtonType.ContactAdd); //TODO change icon
                deleteButton.TranslatesAutoresizingMaskIntoConstraints = false;
                deleteButton.TouchUpInside += (sender, e) => deleteAttachmentClickedAction(this, attachmentDescription);
                AddArrangedSubview(deleteButton);
            }
        }
    }
}
