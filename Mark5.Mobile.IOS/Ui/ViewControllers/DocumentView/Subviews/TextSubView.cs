using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public abstract class TextSubView : DocumentSubView
    {
        UILabel label;
        protected UITextView TextView;

        protected TextSubView(string labelText)
        {
            Initialize(labelText);
        }

        void Initialize(string labelText)
        {
            label = new UILabel();
            label.Text = labelText + ":";
            label.Font = Theme.DefaultFont;
            label.TextColor = UIColor.LightGray;
            label.Opaque = false;
            label.TranslatesAutoresizingMaskIntoConstraints = false;
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
            });

            TextView = new UITextView();
            TextView.Font = Theme.DefaultFont;
            TextView.Editable = false;
            TextView.Opaque = false;
            TextView.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            TextView.AutocorrectionType = UITextAutocorrectionType.Yes;
            TextView.SpellCheckingType = UITextSpellCheckingType.Yes;
            TextView.TextContainer.LineFragmentPadding = 0f;
            TextView.TextContainerInset = UIEdgeInsets.Zero;
            TextView.ClipsToBounds = false;
            TextView.ScrollEnabled = false;
            TextView.TranslatesAutoresizingMaskIntoConstraints = false;
            ContainerView.AddSubview(TextView);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1f, InnerMargin),
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });
        }
    }
}