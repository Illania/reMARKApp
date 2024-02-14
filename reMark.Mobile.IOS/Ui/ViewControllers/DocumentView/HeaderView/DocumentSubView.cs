using System.Threading.Tasks;
using reMark.Mobile.Common.Model;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public abstract class DocumentSubView : UIStackView
    {
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }

        protected UIView ContainerView;

        protected float HorizontalMargin => HeaderView.HorizontalMargin;
        protected float VerticalMargin => HeaderView.VerticalMargin;
        protected float InnerMargin => HeaderView.InnerMargin;
        protected float ExternalVerticalMargin => HeaderView.ExternalVerticalMargin;

        protected DocumentSubView()
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

        public abstract Task RefreshView();

        public abstract void UpdateVisibility();
    }
}
