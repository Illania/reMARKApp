using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public abstract class LargeTextSubView : DocumentSubView
    {
        protected UITextView TextView;

        protected LargeTextSubView()
        {
            Initialize();
        }

        void Initialize()
        {
            TextView = new UITextView();
            TextView.Font = Theme.DefaultFont.WithRelativeSize(4f);
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
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });
        }
    }
}