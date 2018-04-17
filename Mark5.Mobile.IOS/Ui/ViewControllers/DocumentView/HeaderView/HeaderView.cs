using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class HeaderView : UIStackView
    {
        List<DocumentSubView> subViews = new List<DocumentSubView>();

        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }

        FromView fromView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        OriginatorView originatorView;
        SubjectView subjectView;
        DateView dateView;
        ReadByView readByView;
        ReferenceNumberView referenceNumberView;

        public HeaderView()
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = Theme.White;
            Opaque = false;
            Axis = UILayoutConstraintAxis.Vertical;
            Alignment = UIStackViewAlignment.Fill;
            Distribution = UIStackViewDistribution.Fill;
            Spacing = 0f;
            TranslatesAutoresizingMaskIntoConstraints = false;

            subjectView = new SubjectView();
            fromView = new FromView();
            dateView = new DateView();
            toView = new ToView();

            var showMoreButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            showMoreButton.SetTitle("Show more", UIControlState.Normal);
            showMoreButton.TouchUpInside += ShowMoreButton_TouchUpInside;
            showMoreButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            showMoreButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);


            subViews.Add(subjectView);
            subViews.Add(fromView);
            subViews.Add(dateView);
            subViews.Add(toView);

            var firstLine = new UIStackView
            {
                Alignment = UIStackViewAlignment.Center,
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.Fill
            };
            firstLine.AddArrangedSubview(fromView);
            firstLine.AddArrangedSubview(dateView);

            var secondLine = new UIStackView
            {
                Alignment = UIStackViewAlignment.Center,
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.Fill
            };
            secondLine.AddArrangedSubview(toView);
            secondLine.AddArrangedSubview(showMoreButton);

            AddArrangedSubview(subjectView);
            AddArrangedSubview(firstLine);
            AddArrangedSubview(secondLine);

        }

        void ShowMoreButton_TouchUpInside(object sender, EventArgs e)
        {
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
