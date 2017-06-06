using System;
using MailBee.Mime;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class AttachmentsView : MailViewerSubview
    {
        readonly UILabel titleLabel;
        readonly UIStackView stackView;

        public event EventHandler<AttachmentButtonTappedEventArgs> AttachmentTapped = delegate { };

        public AttachmentsView()
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
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });
        }

        public override void RefreshView()
        {
            if (MailMessage == null)
                return;

            stackView.ArrangedSubviews.ForEach(v =>
            {
                stackView.RemoveArrangedSubview(v);
                v.RemoveFromSuperview();
            });

            foreach (var att in MailMessage.Attachments)
            {
                var alssv = new AttachmentsSubView(this, (Attachment) att);
                stackView.AddArrangedSubview(alssv);
            }
        }

        public override void UpdateVisibility()
        {
            if (MailMessage == null)
            {
                Hidden = true;
                return;
            }

            Hidden = MailMessage.Attachments.Count < 1;
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
        public AttachmentsSubView(AttachmentsView view, Attachment attachment)
        {
            Opaque = false;
            TranslatesAutoresizingMaskIntoConstraints = false;

            var attachmentButton = UIButton.FromType(UIButtonType.RoundedRect);
            attachmentButton.TitleLabel.Font = Theme.DefaultFont;
            attachmentButton.SetTitle(attachment.Name + " (" + UI.PrettyFileSize(attachment.Size) + ")", UIControlState.Normal);
            attachmentButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            attachmentButton.TouchUpInside += (sender, e) => view.HandleAttachmentButtonTapped(new AttachmentButtonTappedEventArgs(attachment));
            attachmentButton.Opaque = false;
            attachmentButton.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(attachmentButton);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(attachmentButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(attachmentButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(attachmentButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(attachmentButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0f)
            });
        }
    }

    public class AttachmentButtonTappedEventArgs : EventArgs
    {
        public Attachment Attachment { get; }

        public AttachmentButtonTappedEventArgs(Attachment attachment)
        {
            Attachment = attachment;
        }
    }
}