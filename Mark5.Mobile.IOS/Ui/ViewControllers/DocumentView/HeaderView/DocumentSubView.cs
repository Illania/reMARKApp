using System;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public abstract class DocumentSubView : UIStackView
    {
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }

        protected UIView ContainerView;

        protected float HorizontalMargin = 12f;
        protected float VerticalMargin = 6f;
        protected float InnerMargin = 5f;

        protected DocumentSubView()
        {
            Initialize();
        }

        void Initialize()
        {
            Axis = UILayoutConstraintAxis.Vertical;
            Alignment = UIStackViewAlignment.Fill;
            Distribution = UIStackViewDistribution.Fill;
            Spacing = 0f;
            TranslatesAutoresizingMaskIntoConstraints = false;

            ContainerView = new UIView();
            AddArrangedSubview(ContainerView);
        }

        public abstract void RefreshView();

        public abstract void UpdateVisibility();
    }
}
