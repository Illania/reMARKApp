using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class ExtraFieldsView : DocumentSubView
    {
        UIStackView stackView;

        public ExtraFieldsView()
        {
            InitializeView();
        }

        void InitializeView()
        {
            stackView = new UIStackView
            {
                Opaque = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = InnerMargin,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContainerView.AddSubview(stackView);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });
        }

        public override void RefreshView()
        {
            if (Document == null)
                return;

            stackView.ArrangedSubviews.ForEach(v =>
            {
                stackView.RemoveArrangedSubview(v);
                v.RemoveFromSuperview();
            });

            foreach (var efi in Document.ExtraFields)
            {
                var text = new NSMutableAttributedString(efi.Key.Name + ": " + efi.Value);
                text.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.DarkGray, new NSRange(0, efi.Key.Name.Length + 1));
                var label = new UILabel
                {
                    Font = Theme.DefaultFont,
                    Opaque = false,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                label.AttributedText = text;
                stackView.AddArrangedSubview(label);
            }
        }

        public override void UpdateVisibility()
        {
            if (Document == null)
            {
                Hidden = true;
                return;
            }

            Hidden = !Document.ExtraFields.Any();
        }
    }
}