//
// Project: Mark5.Mobile.IOS
// File: DocumentView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.ViewControllers.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public abstract class DocumentSubView : UIStackView
    {
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public OutgoingDocumentContainer Container { get; set; }

        protected UIView ContainerView;

        protected float HorizontalMargin = 15f;
        protected float VerticalMargin = 12f;
        protected float InnerMargin = 5f;

        protected DocumentSubView()
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.White;
            Opaque = false;
            Axis = UILayoutConstraintAxis.Vertical;
            Alignment = UIStackViewAlignment.Fill;
            Distribution = UIStackViewDistribution.Fill;
            Spacing = 0f;
            TranslatesAutoresizingMaskIntoConstraints = false;

            ContainerView = new UIView();
            AddArrangedSubview(ContainerView);

            AddArrangedSubview(new SeparatorSubView());
        }

        public abstract void RefreshView();

        public abstract void UpdateVisibility();

    }
}
