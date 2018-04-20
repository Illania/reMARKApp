using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class AttachmentsView : DocumentSubView
    {
        const int columnSize = 2;

        UILabel titleLabel;
        UIStackView stackView;
        UIScrollView scrollView;

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
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, 10f),
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
            });

            scrollView = new UIScrollView
            {
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            ContainerView.AddSubview(scrollView);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, titleLabel, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -ExternalVerticalMargin)
            });

            stackView = new UIStackView
            {
                Opaque = false,
                Axis = UILayoutConstraintAxis.Horizontal,
                Alignment = UIStackViewAlignment.Top,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 10f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            scrollView.AddSubview(stackView);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Height, 1f, 0f)
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

            Document.Attachments.Add(new AttachmentDescription
            {
                Name = "SUPER VER LONG NAME", //TODO for testing
                SizeInBytes = 23456,
            });

            foreach (var batch in Document.Attachments.Batch(columnSize))
                stackView.AddArrangedSubview(PrepareColumnStack(batch));
        }

        UIStackView PrepareColumnStack(IEnumerable<AttachmentDescription> batch)
        {
            var columnStack = new UIStackView
            {
                Opaque = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Leading,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            foreach (var ad in batch)
            {
                var alssv = new AttachmentsSubView(this, ad);
                columnStack.AddArrangedSubview(alssv);
            }

            return columnStack;
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

            foreach (var ad in Document.Attachments.Skip(2))
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

            attachmentButton = new UIButton(UIButtonType.RoundedRect)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Left,
                Opaque = false,
                ContentEdgeInsets = new UIEdgeInsets(0.5f, 0.1f, 0.1f, 0.1f),
            };
            attachmentButton.TitleLabel.Font = Theme.DefaultFont;
            attachmentButton.SetTitle(Attachment.Name + " (" + UI.PrettyFileSize(Attachment.SizeInBytes) + ")", UIControlState.Normal);
            attachmentButton.SetContentHuggingPriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Vertical);
            attachmentButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            attachmentButton.TouchUpInside += AttachmentButton_TouchUpInside;
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
