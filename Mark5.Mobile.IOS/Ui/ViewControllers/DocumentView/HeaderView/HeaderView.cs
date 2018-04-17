using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class HeaderView : UIView
    {
        List<DocumentSubView> subViews = new List<DocumentSubView>();

        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }

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
        SeparatorSubView firstSeparator

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

            var showMoreButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            showMoreButton.SetTitle("Show more", UIControlState.Normal);
            showMoreButton.TouchUpInside += ShowMoreButton_TouchUpInside;
            showMoreButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            showMoreButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            showMoreButton.ContentEdgeInsets = new UIEdgeInsets(0, 12f, 0, 12f);
            showMoreButton.BackgroundColor = Theme.Clear;
            showMoreButton.TitleLabel.Font = Theme.DefaultBoldFont;

            subViews.Add(subjectView);
            subViews.Add(fromView);
            subViews.Add(toView);
            subViews.Add(ccView);
            subViews.Add(bccView);
            subViews.Add(dateView);
            subViews.Add(originatorView);
            subViews.Add(readByView);
            subViews.Add(referenceNumberView);

            var firstLine = new UIStackView
            {
                Alignment = UIStackViewAlignment.Center,
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.Fill,
            };
            firstLine.AddArrangedSubview(fromView);
            firstLine.AddArrangedSubview(dateView);

            var secondLine = new UIStackView
            {
                Alignment = UIStackViewAlignment.Center,
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.Fill,
            };
            secondLine.AddArrangedSubview(toView);
            secondLine.AddArrangedSubview(showMoreButton);

            contentView.AddArrangedSubview(subjectView);
            contentView.AddArrangedSubview(firstLine);
            contentView.AddArrangedSubview(secondLine);

            var first

            contentView.AddArrangedSubview(new SeparatorSubView());
            contentView.AddArrangedSubview(ccView);
            contentView.AddArrangedSubview(bccView);
            contentView.AddArrangedSubview(new SeparatorSubView());
            contentView.AddArrangedSubview(readByView);
            contentView.AddArrangedSubview(referenceNumberView);
            contentView.AddArrangedSubview(originatorView);

        }

        void ShowMoreButton_TouchUpInside(object sender, EventArgs e)
        {
            UIView.Animate(0.3, () =>
            {
                contentView.AddArrangedSubview(new SeparatorSubView());
                contentView.AddArrangedSubview(ccView);
                contentView.AddArrangedSubview(bccView);
                contentView.AddArrangedSubview(new SeparatorSubView());
                contentView.AddArrangedSubview(readByView);
                contentView.AddArrangedSubview(referenceNumberView);
                contentView.AddArrangedSubview(originatorView);
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
