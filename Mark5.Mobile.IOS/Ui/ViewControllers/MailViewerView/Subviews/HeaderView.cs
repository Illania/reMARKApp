using System;
using System.Collections.Generic;
using System.Linq;
using MailBee.Mime;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class HeaderView : UIView
    {
        public event EventHandler BeginAnimating = delegate { };
        public event EventHandler Animating = delegate { };
        public event EventHandler EndAnimating = delegate { };
        public event EventHandler<Attachment> AttachmentTapped = delegate { };

        public const float HorizontalMargin = 18f;
        public const float VerticalMargin = 0.5f;
        public const float InnerMargin = 5f;
        public const float ExternalVerticalMargin = 10f;

        public MailMessage MailMessage { get; set; }

        readonly List<MailViewerSubview> subViews = new List<MailViewerSubview>();
        readonly List<MailViewerSubview> hiddenViews = new List<MailViewerSubview>();
        readonly List<MailViewerSubview> animatingSubviews = new List<MailViewerSubview>();

        UIStackView contentView;

        SubjectView subjectView;
        RecipientsView fromView;
        RecipientsView toView;
        RecipientsView ccView;
        RecipientsView bccView;
        RecipientsView replyToView;
        DateView dateView;
        PriorityView priorityView;
        AttachmentsView attachmentsListView;

        SeparatorSubView firstSeparator;
        SeparatorSubView secondSeparator;

        UIButton showMoreButton;

        UIView subHeaderView;
        UIView bottomView;

        NSLayoutConstraint[] compressedConstraints;
        NSLayoutConstraint[] expandedConstraints;

        bool detailsShown;

        public HeaderView()
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = Theme.LightGray;

            contentView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            AddSubview(contentView);
            AddConstraints(new[]
            {
                contentView.TopAnchor.ConstraintEqualTo(this.TopAnchor,ExternalVerticalMargin),
                contentView.LeftAnchor.ConstraintEqualTo(this.LeftAnchor),
                contentView.RightAnchor.ConstraintEqualTo(this.RightAnchor),
                contentView.BottomAnchor.ConstraintEqualTo(this.BottomAnchor)
            });

            subViews.Add(subjectView = new SubjectView());
            subViews.Add(fromView = new RecipientsView(RecipientsView.Type.From));
            subViews.Add(toView = new RecipientsView(RecipientsView.Type.To));
            subViews.Add(ccView = new RecipientsView(RecipientsView.Type.Cc));
            subViews.Add(bccView = new RecipientsView(RecipientsView.Type.Bcc));
            subViews.Add(replyToView = new RecipientsView(RecipientsView.Type.ReplyTo));
            subViews.Add(dateView = new DateView());
            subViews.Add(priorityView = new PriorityView());
            subViews.Add(attachmentsListView = new AttachmentsView());

            firstSeparator = new SeparatorSubView();
            secondSeparator = new SeparatorSubView();
            showMoreButton = GetShowMoreButton();

            subHeaderView = GetSubHeader();
            bottomView = new UIView();
        }

        void PrepareContent()
        {
            contentView.AddArrangedSubview(subjectView);
            contentView.AddArrangedSubview(subHeaderView);
            contentView.AddArrangedSubview(fromView);
            contentView.AddArrangedSubview(toView);
            contentView.AddArrangedSubview(ccView);
            contentView.AddArrangedSubview(bccView);
            contentView.AddArrangedSubview(replyToView);
            contentView.AddArrangedSubview(secondSeparator);
            contentView.AddArrangedSubview(priorityView);
            contentView.AddArrangedSubview(bottomView);
            contentView.AddArrangedSubview(attachmentsListView);

            contentView.AddConstraints(new[]
             {
                bottomView.WidthAnchor.ConstraintEqualTo(contentView.WidthAnchor),
                bottomView.HeightAnchor.ConstraintEqualTo(ExternalVerticalMargin)
            });

            hiddenViews.Add(firstSeparator);
            hiddenViews.Add(ccView);
            hiddenViews.Add(bccView);
            hiddenViews.Add(replyToView);
            hiddenViews.Add(secondSeparator);
            hiddenViews.Add(priorityView);

            hiddenViews.ForEach(v =>
            {
                v.Hidden = true;
                v.Alpha = 0.0f;
            });
        }

        void InitializeHandlers()
        {
            subViews.OfType<RecipientsView>().ForEach(rv =>
            {
                rv.BeginAnimating += RecipientView_BeginAnimating;
                rv.Animating += RecipientView_Animating;
                rv.EndAnimating += RecipientView_EndAnimating;
            });

            attachmentsListView.AttachmentTapped += AttachmentTapped;
        }

        void DeinitializeHandlers()
        {
            subViews.OfType<RecipientsView>().ForEach(rv =>
            {
                rv.BeginAnimating -= RecipientView_BeginAnimating;
                rv.Animating -= RecipientView_Animating;
                rv.EndAnimating -= RecipientView_EndAnimating;
            });

            attachmentsListView.AttachmentTapped -= AttachmentTapped;
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                DeinitializeHandlers();

                contentView.ArrangedSubviews.ForEach(v =>
                {
                    v.RemoveFromSuperview();
                    v = null;
                });

                contentView.RemoveFromSuperview();
                contentView = null;

                animatingSubviews.Clear();
                subViews.Clear();
                hiddenViews.Clear();
            }
        }

        UIButton GetShowMoreButton()
        {
            var button = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            button.SetTitle(Localization.GetString("show_more"), UIControlState.Normal);
            button.TouchUpInside += ShowMoreButton_TouchUpInside;
            button.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            button.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            button.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            button.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            button.ContentEdgeInsets = new UIEdgeInsets(0.1f, 0.1f, 0.1f, 2.0f);
            button.BackgroundColor = Theme.Clear;
            button.TitleLabel.Font = Theme.DefaultBoldFont;

            return button;
        }

        UIView GetSubHeader()
        {
            var container = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            container.AddSubview(dateView);
            container.AddSubview(showMoreButton);
            container.AddSubview(firstSeparator);

            compressedConstraints = new[]
            {
                dateView.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor),
                dateView.TopAnchor.ConstraintEqualTo(container.TopAnchor, VerticalMargin),

                showMoreButton.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor, -HorizontalMargin),
                showMoreButton.TopAnchor.ConstraintEqualTo(dateView.TopAnchor),
                showMoreButton.LeadingAnchor.ConstraintEqualTo(dateView.TrailingAnchor),
                showMoreButton.CenterYAnchor.ConstraintEqualTo(dateView.CenterYAnchor),

                firstSeparator.TopAnchor.ConstraintEqualTo(dateView.BottomAnchor),
                firstSeparator.BottomAnchor.ConstraintEqualTo(container.BottomAnchor, -VerticalMargin),
                firstSeparator.TrailingAnchor.ConstraintEqualTo(showMoreButton.TrailingAnchor),
                firstSeparator.HeightAnchor.ConstraintEqualTo(0f),
                firstSeparator.WidthAnchor.ConstraintEqualTo(0f)
            };

            expandedConstraints = new[]
            {
                dateView.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor),
                dateView.TopAnchor.ConstraintEqualTo(container.TopAnchor),

                showMoreButton.TrailingAnchor.ConstraintEqualTo(container.TrailingAnchor, -HorizontalMargin),
                showMoreButton.TopAnchor.ConstraintEqualTo(dateView.TopAnchor),
                showMoreButton.LeadingAnchor.ConstraintEqualTo(dateView.TrailingAnchor),
                showMoreButton.CenterYAnchor.ConstraintEqualTo(dateView.CenterYAnchor),

                firstSeparator.TopAnchor.ConstraintEqualTo(showMoreButton.BottomAnchor),
                firstSeparator.BottomAnchor.ConstraintEqualTo(container.BottomAnchor, -VerticalMargin),
                firstSeparator.TrailingAnchor.ConstraintEqualTo(showMoreButton.TrailingAnchor),
                firstSeparator.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor)
            };

            container.AddConstraints(compressedConstraints);

            return container;
        }

        #region Handlers

        void ShowMoreButton_TouchUpInside(object sender, EventArgs e)
        {
            LayoutIfNeeded();
            Animate(0.5, ShowHideDetailsAction);
        }

        void ShowHideDetailsAction()
        {
            if (!detailsShown)
            {
                showMoreButton.SetTitle(Localization.GetString("show_less"), UIControlState.Normal);
                hiddenViews.ForEach(v =>
                {
                    v.UpdateVisibility();
                    v.Alpha = 1.0f;
                });

                subHeaderView.RemoveConstraints(compressedConstraints);
                subHeaderView.AddConstraints(expandedConstraints);

                subViews.OfType<RecipientsView>().ForEach(r => r.ExpandCompressView());
            }
            else
            {
                showMoreButton.SetTitle(Localization.GetString("show_more"), UIControlState.Normal);

                hiddenViews.ForEach(v =>
                {
                    v.Hidden = true;
                    v.Alpha = 0.0f;
                });

                subHeaderView.RemoveConstraints(expandedConstraints);
                subHeaderView.AddConstraints(compressedConstraints);

                subViews.OfType<RecipientsView>().ForEach(r => r.ExpandCompressView());
            }

            detailsShown = !detailsShown;
            LayoutIfNeeded();
        }

        void RecipientView_BeginAnimating(object sender, EventArgs e)
        {
            if (!animatingSubviews.Any())
                BeginAnimating(this, EventArgs.Empty);

            var subview = (MailViewerSubview)sender;
            if (!animatingSubviews.Contains(subview))
                animatingSubviews.Add(subview);
        }

        void RecipientView_Animating(object sender, EventArgs e)
        {
            Animating(this, EventArgs.Empty);
        }

        void RecipientView_EndAnimating(object sender, EventArgs e)
        {
            var subview = (MailViewerSubview)sender;
            if (animatingSubviews.Contains(subview))
                animatingSubviews.Remove(subview);

            if (!animatingSubviews.Any())
                EndAnimating(this, EventArgs.Empty);
        }

        #endregion

        #region Public methods

        public void RefreshHeader()
        {
            DeinitializeHandlers();
            InitializeHandlers();

            foreach (var view in subViews)
            {
                view.MailMessage = MailMessage;
                view.RefreshView();
            }

            PrepareContent();
            attachmentsListView.UpdateVisibility();
        }

        #endregion

    }
}
