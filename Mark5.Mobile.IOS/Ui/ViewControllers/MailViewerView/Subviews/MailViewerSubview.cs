using MailBee.Mime;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public abstract class MailViewerSubview : UIStackView
    {
        public MailMessage MailMessage { get; set; }

        protected UIView ContainerView;

        protected float HorizontalMargin = 15f;
        protected float VerticalMargin = 12f;
        protected float InnerMargin = 5f;

        protected MailViewerSubview()
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