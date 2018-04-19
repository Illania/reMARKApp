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
        static public float HorizontalMargin = 18f;
        static public float VerticalMargin = 2f;
        static public float InnerMargin = 5f;
        static public float ExternalVerticalMargin = 10f;

        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }

        readonly List<DocumentSubView> subViews = new List<DocumentSubView>();
        readonly List<DocumentSubView> hiddenViews = new List<DocumentSubView>();

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
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, ExternalVerticalMargin),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, 0)
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
            var bottomView = new UIView();

            contentView.AddArrangedSubview(subjectView);
            contentView.AddArrangedSubview(subHeaderView);
            contentView.AddArrangedSubview(fromView);
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
                NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, contentView, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ExternalVerticalMargin),
            });

            hiddenViews.Add(firstSeparator);
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
            button.ContentEdgeInsets = new UIEdgeInsets(0f, 0.1f, 0f, 0);
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
                NSLayoutConstraint.Create(dateView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, container, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(dateView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Top, 1f, 0f),

                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, container, NSLayoutAttribute.Trailing, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, dateView, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, dateView, NSLayoutAttribute.CenterY, 1f, 0f),

                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Top, NSLayoutRelation.Equal, dateView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, container, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0f),
            };

            expandedConstraints = new[]
            {
                NSLayoutConstraint.Create(dateView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, container, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(dateView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Top, 1f, 0f),

                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, container, NSLayoutAttribute.Trailing, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, dateView, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, dateView, NSLayoutAttribute.CenterY, 1f, 0f),

                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Top, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, container, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, container, NSLayoutAttribute.Leading, 1f, 0f),
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

        public void RefreshHeader()
        {
            foreach (var view in subViews)
            {
                view.Document = Document;
                view.DocumentPreview = DocumentPreview;
                view.RefreshView();
            }

            attachmentsListView.UpdateVisibility();
        }
    }
}
