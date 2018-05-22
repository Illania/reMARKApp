using System;
using System.Collections.Generic;
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
            ContainerView.BackgroundColor = Theme.White;

            titleLabel = new UILabel
            {
                Text = Localization.GetString("attachments") + ":",
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContainerView.AddSubview(titleLabel);
            ContainerView.AddConstraints(new[]
            {
                titleLabel.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, 10f),
                titleLabel.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin),
            });

            scrollView = new UIScrollView
            {
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ShowsHorizontalScrollIndicator = false,
            };
            ContainerView.AddSubview(scrollView);
            ContainerView.AddConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor, 1f),
                scrollView.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor),
                scrollView.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor),
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
                stackView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor),
                stackView.LeftAnchor.ConstraintEqualTo(scrollView.LeftAnchor, HorizontalMargin),
                stackView.RightAnchor.ConstraintEqualTo(scrollView.RightAnchor, -HorizontalMargin),
                stackView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor),
                stackView.HeightAnchor.ConstraintEqualTo(scrollView.HeightAnchor),
            });
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                titleLabel?.RemoveFromSuperview();
                titleLabel = null;

                stackView?.RemoveFromSuperview();
                stackView = null;
            }
        }

        public override void RefreshView()
        {
            if (Document == null)
                return;

            stackView.ArrangedSubviews.ForEach(v => v.RemoveFromSuperview());
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
                Spacing = 5f,
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
                ContentEdgeInsets = new UIEdgeInsets(3.5f, 7.5f, 3.5f, 7.5f),
                BackgroundColor = Theme.Gray,
            };
            attachmentButton.TitleLabel.Font = Theme.DefaultFont;
            attachmentButton.Layer.CornerRadius = 5f;
            attachmentButton.SetTitle(Attachment.Name + " (" + UI.PrettyFileSize(Attachment.SizeInBytes) + ")", UIControlState.Normal);
            attachmentButton.SetContentHuggingPriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Vertical);
            attachmentButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            attachmentButton.TouchUpInside += AttachmentButton_TouchUpInside;
            AddSubview(attachmentButton);
            AddConstraints(new[]
            {
                attachmentButton.TopAnchor.ConstraintEqualTo(TopAnchor),
                attachmentButton.LeftAnchor.ConstraintEqualTo(LeftAnchor),
                attachmentButton.BottomAnchor.ConstraintEqualTo(BottomAnchor),
                attachmentButton.RightAnchor.ConstraintEqualTo(RightAnchor),
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
