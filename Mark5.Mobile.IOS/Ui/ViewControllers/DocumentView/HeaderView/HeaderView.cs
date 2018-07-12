using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class HeaderView : UIView
    {
        public event EventHandler<RecipientTappedEventArgs> RecipientTapped = delegate { };
        public event EventHandler<AttachmentButtonTappedEventArgs> AttachmentTapped = delegate { };

        public event EventHandler BeginAnimating = delegate { };
        public event EventHandler Animating = delegate { };
        public event EventHandler EndAnimating = delegate { };

        public const float HorizontalMargin = 18f;
        public const float VerticalMargin = 0.5f;
        public const float InnerMargin = 5f;
        public const float ExternalVerticalMargin = 10f;

        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }

        readonly List<DocumentSubView> subViews = new List<DocumentSubView>();
        readonly List<DocumentSubView> hiddenViews = new List<DocumentSubView>();
        readonly List<DocumentSubView> animatingSubviews = new List<DocumentSubView>();

        UIStackView contentView;

        SubjectView subjectView;
        FromView fromView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        DateView dateView;
        ReadByView readByView;
        ReferenceNumberView referenceNumberView;
        PriorityView priorityView;
        OriginatorView originatorView;
        CreatorView creatorView;
        ExtraFieldsView extraFieldsView;
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
            subViews.Add(fromView = new FromView());
            subViews.Add(toView = new ToView());
            subViews.Add(ccView = new CcView());
            subViews.Add(bccView = new BccView());
            subViews.Add(dateView = new DateView());
            subViews.Add(readByView = new ReadByView());
            subViews.Add(referenceNumberView = new ReferenceNumberView());
            subViews.Add(priorityView = new PriorityView());
            subViews.Add(originatorView = new OriginatorView());
            subViews.Add(creatorView = new CreatorView());
            subViews.Add(extraFieldsView = new ExtraFieldsView());
            subViews.Add(attachmentsListView = new AttachmentsView());

            firstSeparator = new SeparatorSubView();
            secondSeparator = new SeparatorSubView();
            showMoreButton = CreateShowMoreButton();

            subHeaderView = GetSubHeader();
            bottomView = new UIView();
        }

        void PrepareContent()
        {
            contentView.AddArrangedSubview(subjectView);
            contentView.AddArrangedSubview(subHeaderView);
            contentView.AddArrangedSubview(fromView);
            if (!toView.IsEmpty())
                contentView.AddArrangedSubview(toView);
            contentView.AddArrangedSubview(ccView);
            contentView.AddArrangedSubview(bccView);
            contentView.AddArrangedSubview(secondSeparator);
            contentView.AddArrangedSubview(priorityView);
            contentView.AddArrangedSubview(readByView);
            contentView.AddArrangedSubview(referenceNumberView);
            contentView.AddArrangedSubview(creatorView);
            contentView.AddArrangedSubview(originatorView);
            contentView.AddArrangedSubview(extraFieldsView);
            contentView.AddArrangedSubview(bottomView);
            contentView.AddArrangedSubview(attachmentsListView);

            contentView.AddConstraints(new[]
             {
                bottomView.WidthAnchor.ConstraintEqualTo(contentView.WidthAnchor),
                bottomView.HeightAnchor.ConstraintEqualTo(ExternalVerticalMargin)
            });

            hiddenViews.Add(firstSeparator);
            if (!toView.IsEmpty())
                hiddenViews.Add(ccView);
            hiddenViews.Add(bccView);
            hiddenViews.Add(secondSeparator);
            hiddenViews.Add(priorityView);
            hiddenViews.Add(readByView);
            hiddenViews.Add(referenceNumberView);
            hiddenViews.Add(creatorView);
            hiddenViews.Add(originatorView);
            hiddenViews.Add(extraFieldsView);

            hiddenViews.ForEach(v =>
            {
                v.Hidden = true;
                v.Alpha = 0.0f;
            });

            if (toView.IsEmpty())
                subViews.Remove(toView);
        }

        void InitializeHandlers()
        {
            subViews.OfType<RecipientsView>().ForEach(rv =>
            {
                rv.RecipientTapped += RecipientTapped;
                rv.BeginAnimating += RecipientView_BeginAnimating;
                rv.Animating += RecipientView_Animating;
                rv.EndAnimating += RecipientView_EndAnimating;
            });

            if (attachmentsListView != null)
                attachmentsListView.AttachmentTapped += AttachmentTapped;
        }

        void DeinitializeHandlers()
        {
            subViews.OfType<RecipientsView>().ForEach(rv =>
            {
                rv.RecipientTapped -= RecipientTapped;
                rv.BeginAnimating -= RecipientView_BeginAnimating;
                rv.Animating -= RecipientView_Animating;
                rv.EndAnimating -= RecipientView_EndAnimating;
            });

            if (attachmentsListView != null)
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

        UIButton CreateShowMoreButton()
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

            var subview = (DocumentSubView)sender;
            if (!animatingSubviews.Contains(subview))
                animatingSubviews.Add(subview);
        }

        void RecipientView_Animating(object sender, EventArgs e)
        {
            Animating(this, EventArgs.Empty);
        }

        void RecipientView_EndAnimating(object sender, EventArgs e)
        {
            var subview = (DocumentSubView)sender;
            if (animatingSubviews.Contains(subview))
                animatingSubviews.Remove(subview);

            if (!animatingSubviews.Any())
                EndAnimating(this, EventArgs.Empty);
        }


        #region Public methods

        public void RefreshHeader()
        {
            DeinitializeHandlers();
            InitializeHandlers();

            foreach (var view in subViews)
            {
                view.Document = Document;
                view.DocumentPreview = DocumentPreview;
                view.RefreshView();
            }

            PrepareContent();
            attachmentsListView.UpdateVisibility();
        }

        public void UpdatePriority()
        {
            if (priorityView == null)
                return;

            priorityView.RefreshView();

            if (detailsShown)
                Animate(0.3, priorityView.UpdateVisibility);
        }

        public void UpdateReadBy()
        {
            if (readByView == null)
                return;

            readByView.RefreshView();

            if (detailsShown)
                Animate(0.3, readByView.UpdateVisibility);
        }

        #endregion

    }
}
