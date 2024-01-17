using System;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public abstract class TextSubView : DocumentSubView
    {
        UILabelScalable label;
        protected UITextViewScalable TextView;

        protected TextSubView(string labelText)
        {
            Initialize(labelText);
        }

        void Initialize(string labelText)
        {
            label = new UILabelScalable
            {
                Text = labelText + ":",
                Font = Theme.DefaultFont.CustomFont(),
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

            TextView = new UITextViewScalable
            {
                BackgroundColor = Theme.Clear,
                Font = Theme.DefaultFont.CustomFont(),
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
