using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class HeaderView : UIView
    {
        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }

        readonly List<DocumentSubView> subViews = new List<DocumentSubView>();
        readonly List<UIView> hiddenViews = new List<UIView>();

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

        public HeaderView()
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = Theme.LightGray;

            contentView = new UIStackView()
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
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 0),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, 0)
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

            subViews.Add(subjectView);
            subViews.Add(fromView);
            subViews.Add(toView);
            subViews.Add(ccView);
            subViews.Add(bccView);
            subViews.Add(dateView);
            subViews.Add(originatorView);
            subViews.Add(readByView);
            subViews.Add(referenceNumberView);

            var firstLine = PrepareFirstLine();
            var secondLine = PrepareSecondLine();

            contentView.AddArrangedSubview(subjectView);
            contentView.AddArrangedSubview(firstLine);
            contentView.AddArrangedSubview(secondLine);

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

            hiddenViews.ForEach(v => v.Hidden = true);
        }

        UIView lineContainer;
        UIButton showMoreButton;

        NSLayoutConstraint[] compressedConstraints;
        NSLayoutConstraint[] expandedConstraints;
        bool shown;

        UIView PrepareFirstLine()
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

        UIView PrepareSecondLine()
        {
            showMoreButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            showMoreButton.SetTitle("Show more", UIControlState.Normal);
            showMoreButton.TouchUpInside += ShowMoreButton_TouchUpInside;
            showMoreButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            showMoreButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            showMoreButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            showMoreButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            showMoreButton.ContentEdgeInsets = new UIEdgeInsets(0, 12f, 0, 12f);
            showMoreButton.BackgroundColor = Theme.Clear;
            showMoreButton.TitleLabel.Font = Theme.DefaultBoldFont;

            lineContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            lineContainer.AddSubview(showMoreButton);
            lineContainer.AddSubview(toView);
            lineContainer.AddSubview(firstSeparator);


            compressedConstraints = new[]
            {
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Top, 1f, 0f),

                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Top, 1f, 0f),

                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Leading, 1f, -6f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.CenterY, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0f),
            };

            expandedConstraints = new[]
            {
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(toView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Trailing, 1f, 0f),

                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(showMoreButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Top, 1f, 0f),

                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, lineContainer, NSLayoutAttribute.Leading, 1f, 0f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.Leading, 1f, -6f),
                NSLayoutConstraint.Create(firstSeparator, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, showMoreButton, NSLayoutAttribute.CenterY, 1f, 0f),
            };

            lineContainer.AddConstraints(compressedConstraints);

            return lineContainer;
        }

        void ShowMoreButton_TouchUpInside(object sender, EventArgs e)
        {
            LayoutIfNeeded();

            Animate(0.5, () =>
            {
                if (!shown)
                {
                    showMoreButton.SetTitle("Show less", UIControlState.Normal);
                    hiddenViews.ForEach(v => v.Hidden = false);

                    lineContainer.RemoveConstraints(compressedConstraints);
                    lineContainer.AddConstraints(expandedConstraints);
                }
                else
                {
                    showMoreButton.SetTitle("Show more", UIControlState.Normal);

                    hiddenViews.ForEach(v => v.Hidden = true);

                    lineContainer.RemoveConstraints(expandedConstraints);
                    lineContainer.AddConstraints(compressedConstraints);
                }

                shown = !shown;
                LayoutIfNeeded();
            });

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
