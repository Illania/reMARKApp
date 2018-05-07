using Mark5.Mobile.Common.Model;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
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

        public abstract void RefreshView();

        public abstract void UpdateVisibility();

        //TODO BIG TODO
        // - Try to change the padding on the web view controller following the animation 
        // - Test on iPad
        // - Need to find a solution to the disappearing text -- Probably could leave it like this, we never had problems before
        // - Maybe we could have a "shortcode" subview on the compose view, so it's easier to remove / add shortcodes 
        // - At the moment the arrow doesn't appear if not needed. Show to Una in case we need to do something different
    }
}
