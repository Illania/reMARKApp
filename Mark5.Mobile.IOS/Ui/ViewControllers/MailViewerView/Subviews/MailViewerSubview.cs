using MailBee.Mime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public abstract class MailViewerSubview : UIStackView
    {
        public MailMessage MailMessage { get; set; }

        protected UIView ContainerView;

        protected float HorizontalMargin => HeaderView.HorizontalMargin;
        protected float VerticalMargin => HeaderView.VerticalMargin;
        protected float InnerMargin => HeaderView.InnerMargin;
        protected float ExternalVerticalMargin => HeaderView.ExternalVerticalMargin;

        protected MailViewerSubview()
        {
            Initialize();
        }

        void Initialize()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            Axis = UILayoutConstraintAxis.Vertical;
            Alignment = UIStackViewAlignment.Fill;
            Distribution = UIStackViewDistribution.Fill;
            Spacing = 0f;

            ContainerView = new UIView();
            AddArrangedSubview(ContainerView);
        }

        public abstract void RefreshView();

        public abstract void UpdateVisibility();
    }
}