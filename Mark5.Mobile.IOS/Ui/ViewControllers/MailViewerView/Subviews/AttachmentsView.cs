using System;
using MailBee.Mime;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class AttachmentsView : MailViewerSubview
    {
        readonly WeakReference<MailViewerViewController> mailViewerViewControllerWeakReference;
        UILabel titleLabel;
        UIStackView stackView;

        public event EventHandler<Attachment> AttachmentTapped = delegate { };

        public AttachmentsView()
        {
            titleLabel = new UILabel
            {
                Text = Localization.GetString("attachments") + ":",
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
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

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                titleLabel?.RemoveFromSuperview();
                titleLabel = null;

                stackView?.RemoveFromSuperview();
                foreach (var v in stackView.ArrangedSubviews)
                    v.RemoveFromSuperview();
                stackView = null;
            }
        }

        public override void RefreshView()
        {
            if (MailMessage == null)
                return;

            stackView.ArrangedSubviews.ForEach(v => v.RemoveFromSuperview());

            foreach (var att in MailMessage.Attachments)
            {
                var alssv = new AttachmentsSubView(this, (Attachment)att);
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

        public void OpenAttachment(Attachment attachment) => AttachmentTapped?.Invoke(this, attachment);
    }

    class AttachmentsSubView : UIView
    {
        readonly WeakReference<AttachmentsView> viewWeakReference;
        readonly Attachment attachment;

        readonly UIButton attachmentButton;

        public AttachmentsSubView(AttachmentsView attachmentsView, Attachment attachment)
        {
            this.attachment = attachment;
            this.viewWeakReference = attachmentsView.Wrap();

            TranslatesAutoresizingMaskIntoConstraints = false;

            attachmentButton = new UIButton(UIButtonType.RoundedRect);
            attachmentButton.TitleLabel.Font = Theme.DefaultFont;
            attachmentButton.SetTitle(attachment.Name + " (" + UI.PrettyFileSize(attachment.Size) + ")", UIControlState.Normal);
            attachmentButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
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

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper != null)
            {
                if (attachmentButton != null)
                    attachmentButton.TouchUpInside += AttachmentButton_TouchUpInside;
            }
            else
            {
                if (attachmentButton != null)
                    attachmentButton.TouchUpInside -= AttachmentButton_TouchUpInside;
            }
        }

        void AttachmentButton_TouchUpInside(object sender, EventArgs e) => viewWeakReference.Unwrap()?.OpenAttachment(attachment);
    }
}