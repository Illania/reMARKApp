using System;
using System.Linq;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class AttachmentsView : DocumentSubView
    {
        UILabel titleLabel;
        UIStackView stackView;

        public event EventHandler<AttachmentButtonTappedEventArgs> AttachmentTapped = delegate { };

        public AttachmentsView()
        {
            titleLabel = new UILabel
            {
                Text = Localization.GetString("attachments") + ":",
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContainerView.BackgroundColor = Theme.White;
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

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                titleLabel?.RemoveFromSuperview();
                titleLabel = null;

                stackView?.RemoveFromSuperview();
                foreach (var v in stackView.ArrangedSubviews)
                {
                    if (v is UIButton b)
                        b.TouchUpInside -= HandleShowMoreButtonTapped;
                    v.RemoveFromSuperview();
                }
                stackView = null;
            }
        }

        public override void RefreshView()
        {
            if (Document == null)
                return;

            foreach (var v in stackView.ArrangedSubviews)
            {
                if (v is UIButton b)
                    b.TouchUpInside -= HandleShowMoreButtonTapped;
                v.RemoveFromSuperview();
            };

            if (Document.Attachments.Count > 4)
            {
                foreach (var ad in Document.Attachments.Take(3))
                {
                    var alssv = new AttachmentsSubView(this, ad);
                    stackView.AddArrangedSubview(alssv);
                }

                var showMoreButton = new UIButton(UIButtonType.RoundedRect);
                showMoreButton.SetTitle(Localization.GetString("show_more___"), UIControlState.Normal);
                showMoreButton.TintColor = Theme.DarkBlue;
                showMoreButton.TouchUpInside += HandleShowMoreButtonTapped;
                stackView.AddArrangedSubview(showMoreButton);
            }
            else
            {
                foreach (var ad in Document.Attachments)
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

            Hidden = Document.Attachments.Count < 1;
        }

        #region Event handlers

        public void HandleAttachmentButtonTapped(AttachmentButtonTappedEventArgs eventArgs) => AttachmentTapped?.Invoke(this, eventArgs);

        void HandleShowMoreButtonTapped(object sender, EventArgs e)
        {
            var btn = (UIButton)sender;
            btn.TouchUpInside -= HandleShowMoreButtonTapped;
            btn.RemoveFromSuperview();

            foreach (var ad in Document.Attachments.Skip(3))
            {
                var alssv = new AttachmentsSubView(this, ad);
                stackView.AddArrangedSubview(alssv);
            }
        }

        #endregion

    }

    class AttachmentsSubView : UIView
    {
        public AttachmentDescription Attachment { get; }

        WeakReference<AttachmentsView> viewWeakReference;
        UIButton attachmentButton;

        public AttachmentsSubView(AttachmentsView view, AttachmentDescription attachmentDescription)
        {
            viewWeakReference = view.Wrap();
            Attachment = attachmentDescription;

            InitSubViews();
        }

        void InitSubViews()
        {
            Opaque = false;
            TranslatesAutoresizingMaskIntoConstraints = false;

            attachmentButton = new UIButton(UIButtonType.RoundedRect);
            attachmentButton.TitleLabel.Font = Theme.DefaultFont;
            attachmentButton.SetTitle(Attachment.Name + " (" + UI.PrettyFileSize(Attachment.SizeInBytes) + ")", UIControlState.Normal);
            attachmentButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            attachmentButton.TouchUpInside += AttachmentButton_TouchUpInside;
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

        void AttachmentButton_TouchUpInside(object sender, EventArgs e) => viewWeakReference.Unwrap()?.HandleAttachmentButtonTapped(new AttachmentButtonTappedEventArgs(Attachment));

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                attachmentButton.RemoveFromSuperview();
                attachmentButton.TouchUpInside -= AttachmentButton_TouchUpInside;
                attachmentButton = null;
            }
        }
    }

    public class AttachmentButtonTappedEventArgs : EventArgs
    {
        public AttachmentDescription Attachment { get; }

        public AttachmentButtonTappedEventArgs(AttachmentDescription attachment)
        {
            Attachment = attachment;
        }
    }
}
