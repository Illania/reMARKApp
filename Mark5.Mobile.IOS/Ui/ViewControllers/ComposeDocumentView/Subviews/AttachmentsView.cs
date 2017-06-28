using System;
using System.Collections.Generic;
using System.IO;
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

        List<AttachmentDescription> attachmentsDescription = new List<AttachmentDescription>();

        public event EventHandler<AttachmentDescription> AttachmentClicked = delegate { };
        public event EventHandler<AttachmentDescription> DeleteAttachmentClicked = delegate { };

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
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
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
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, titleLabel, NSLayoutAttribute.Bottom, 1f, InnerMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin - InnerMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });

            Hidden = true;
        }

        #region Public methods

        public override Task InitializeView()
        {
            if (RestoreWorkingCopy)
            {
                foreach (var attachmentDescription in Document.Attachments)
                    AddAttachment(attachmentDescription);
                
                return Task.CompletedTask;
            }

            attachmentsDescription.Clear();
            stackView.Subviews.OfType<AttachmentsSubView>().ForEach(v => v.RemoveFromSuperview());

            if (CreationModeFlag == DocumentCreationModeFlag.Forward || CreationModeFlag == DocumentCreationModeFlag.Edit
                || CreationModeFlag == DocumentCreationModeFlag.New
                && (CopyToNewOptions == CopyToNewOption.KeepOnlyAttachments || CopyToNewOptions == CopyToNewOption.KeepTextAndAttachments))
            {
                foreach (var attachmentDescription in PreviousDocument.Attachments)
                    AddAttachment(attachmentDescription);
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            var remoteAttachments = attachmentsDescription.OfType<AttachmentDescription>().ToList();
            Document.Attachments.Clear();
            Document.Attachments.AddRange(remoteAttachments);
            DocumentPreview.AttachmentsCount = attachmentsDescription.Count;

            return Task.CompletedTask;
        }

        public void AddAttachment(AttachmentDescription attachment)
        {
            attachmentsDescription.Add(attachment);

            var alssv = new AttachmentsSubView(attachment, HandleAttachmentClicked, HandleDeleteAttachmentClicked);
            stackView.AddArrangedSubview(alssv);

            UpdateVisibility();
        }

        public void RemoveAttachment(object view, AttachmentDescription attachmentDescrption)
        {
            attachmentsDescription.Remove(attachmentDescrption);
            (view as UIView).RemoveFromSuperview();

            UpdateVisibility();
        }

        #endregion

        #region Utilities

        void UpdateVisibility() => Hidden = attachmentsDescription.Count < 1;

        void HandleAttachmentClicked(AttachmentsSubView view, AttachmentDescription attachmentDescrption) => AttachmentClicked(view, attachmentDescrption);

        void HandleDeleteAttachmentClicked(AttachmentsSubView view, AttachmentDescription attachmentDescrption) => DeleteAttachmentClicked(view, attachmentDescrption);

        #endregion

        class AttachmentsSubView : UIStackView
        {
            readonly AttachmentDescription attachmentDescription;
            readonly Action<AttachmentsSubView, AttachmentDescription> attachmentClickedAction;
            readonly Action<AttachmentsSubView, AttachmentDescription> deleteAttachmentClickedAction;

            UIButton filenameButton;
            UIButton deleteButton;

            public AttachmentsSubView(AttachmentDescription attachmentDescription, Action<AttachmentsSubView, AttachmentDescription> attachmentClickedAction, Action<AttachmentsSubView, AttachmentDescription> deleteAttachmentClickedAction)
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

                deleteButton = new UIButton();
                deleteButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "remove.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                deleteButton.BackgroundColor = UIColor.Clear;
                deleteButton.TranslatesAutoresizingMaskIntoConstraints = false;
                deleteButton.ContentEdgeInsets = new UIEdgeInsets(5.0f, 5.0f, 5.0f, 5.0f);
                deleteButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                deleteButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                deleteButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                deleteButton.TouchUpInside += (sender, e) => deleteAttachmentClickedAction(this, attachmentDescription);
                AddArrangedSubview(deleteButton);
            }
        }
    }
}