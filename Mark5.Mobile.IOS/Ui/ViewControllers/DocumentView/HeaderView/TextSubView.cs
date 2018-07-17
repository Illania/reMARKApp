using System;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
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
            label = new UILabel
            {
                Text = labelText + ":",
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                label.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                label.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin)
            });

            TextView = new UITextView
            {
                BackgroundColor = Theme.Clear,
                Font = Theme.DefaultFont,
                Editable = false,
                Opaque = false,
                AutocapitalizationType = UITextAutocapitalizationType.Sentences,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                SpellCheckingType = UITextSpellCheckingType.Yes,
                TextContainerInset = UIEdgeInsets.Zero,
                ClipsToBounds = false,
                ScrollEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            TextView.TextContainer.LineFragmentPadding = 0f;
            ContainerView.AddSubview(TextView);
            ContainerView.AddConstraints(new[]
            {
                TextView.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                TextView.LeftAnchor.ConstraintEqualTo(label.RightAnchor, InnerMargin),
                TextView.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                TextView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                label?.RemoveFromSuperview();
                label = null;

                TextView?.RemoveFromSuperview();
                TextView = null;
            }
        }
    }
}
