using System;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.AddEditContactView.Subviews
{
    public abstract class AddEditContactSubView : UIStackView
    {
        public event EventHandler Edited = delegate { };

        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public ContactPreview ParentContactPreview { get; set; }
        public ContactCreationModeFlag CreationMode { get; set; }

        protected UIView ContainerView;

        protected float HorizontalMargin = 15f;
        protected float VerticalMargin = 12f;
        protected float InnerMargin = 5f;

        protected AddEditContactSubView()
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


        //This needs to be used by all subclasses that can contain invalid content
        protected void OnContentChanged()
        {
            Edited(this, EventArgs.Empty);
        }

        abstract public void RefreshView();
        abstract public bool ContainsValidContent();
    }
}
