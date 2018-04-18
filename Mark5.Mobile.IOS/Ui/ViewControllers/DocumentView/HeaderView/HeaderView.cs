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
        static public float TopMargin = 10f;

        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }

        readonly List<DocumentSubView> subViews = new List<DocumentSubView>();
        readonly List<DocumentSubView> hiddenViews = new List<DocumentSubView>();

        UIStackView contentView;

        FromView fromView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        OriginatorView originatorView;
        SubjectView subjectView;
        DateView dateView;
        ReadByView readByView;
        ReferenceNumberView referenceNumberView;

        SeparatorSubView firstSeparator;
        SeparatorSubView secondSeparator;

        UIView toShowMoreContainer;
        UIButton showMoreButton;

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
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, TopMargin),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, -TopMargin)
            });

            subjectView = new SubjectView();
            fromView = new FromView();
            ccView = new CcView();
            bccView = new BccView();
            toView = new ToView();
            dateView = new DateView();
            originatorView = new OriginatorView();
            readByView = new ReadByView();
            referenceNumberView = new ReferenceNumberView();
            firstSeparator = new SeparatorSubView();
            secondSeparator = new SeparatorSubView();
            showMoreButton = CreateShowMoreButton();

            subViews.Add(subjectView);
            subViews.Add(fromView);
            subViews.Add(toView);
            subViews.Add(ccView);
            subViews.Add(bccView);
            subViews.Add(dateView);
            subViews.Add(originatorView);
            subViews.Add(readByView);
            subViews.Add(referenceNumberView);


            var fromDateView = CreateFromDateView();
            var toShowMoreView = CreateToShowMoreView();

            contentView.AddArrangedSubview(subjectView);
            contentView.AddArrangedSubview(fromDateView);
            contentView.AddArrangedSubview(toShowMoreView);

            contentView.AddArrangedSubview(ccView);
            contentView.AddArrangedSubview(bccView);
            contentView.AddArrangedSubview(secondSeparator);
            contentView.AddArrangedSubview(readByView);
            contentView.AddArrangedSubview(referenceNumberView);
            contentView.AddArrangedSubview(originatorView);

            hiddenViews.Add(ccView);
            hiddenViews.Add(bccView);
            hiddenViews.Add(secondSeparator);
            hiddenViews.Add(readByView);
            hiddenViews.Add(referenceNumberView);
            hiddenViews.Add(originatorView);

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
            button.SetTitle("Show more", UIControlState.Normal);
            button.TouchUpInside += ShowMoreButton_TouchUpInside;
            button.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            button.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            button.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            button.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            button.ContentEdgeInsets = new UIEdgeInsets(0f, 0.1f, 0f, 0f);
            button.BackgroundColor = Theme.Clear;
            button.TitleLabel.Font = Theme.DefaultBoldFont;

            return button;
        }

        UIView CreateFromDateView()
        {
            var firstLine = new UIStackView
            {
                Alignment = UIStackViewAlignment.Center,
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.Fill,
            };
            firstLine.AddArrangedSubview(fromView);
            firstLine.AddArrangedSubview(dateView);

            return firstLine;
        }

        //TODO start with new font name and type

        UIView CreateToShowMoreView()
        {
            toShowMoreContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            PrepareConstraints();

            toShowMoreContainer.AddSubview(showMoreButton);
            toShowMoreContainer.AddSubview(toView);
            toShowMoreContainer.AddSubview(firstSeparator);

            toShowMoreContainer.AddConstraints(compressedConstraints);

            return toShowMoreContainer;
        }

        void PrepareConstraints()
        {
            compressedConstraints = new[]
            {
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Top, 1f, 0f),

                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Trailing, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.CenterY, 1f, 0f),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Bottom, 1f, 0f),

                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Leading, 1f, -6f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.CenterY, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0f),
            };

            expandedConstraints = new[]
            {
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Trailing, 1f, 0f),

                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Trailing, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Top, 1f, 6f),

                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, toShowMoreContainer, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Leading, 1f, -6f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.CenterY, 1f, 0f),
            };
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
                showMoreButton.SetTitle("Show less", UIControlState.Normal); //TODO need to get string from localization
                hiddenViews.ForEach(v =>
                {
                    v.UpdateVisibility();
                    v.Alpha = 1.0f;
                });

                toShowMoreContainer.RemoveConstraints(compressedConstraints);
                toShowMoreContainer.AddConstraints(expandedConstraints);

                subViews.OfType<RecipientsView>().ForEach(r => r.ExpandCompressView());
            }
            else
            {
                showMoreButton.SetTitle("Show more", UIControlState.Normal);

                hiddenViews.ForEach(v =>
                {
                    v.Hidden = true;
                    v.Alpha = 0.0f;
                });

                toShowMoreContainer.RemoveConstraints(expandedConstraints);
                toShowMoreContainer.AddConstraints(compressedConstraints);

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
        }
    }
}
