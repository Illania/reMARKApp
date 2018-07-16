using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class ExtraFieldsView : DocumentSubView
    {
        UIStackView stackView;

        public ExtraFieldsView()
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
                stackView.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor,HorizontalMargin),
                stackView.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor,HorizontalMargin),
                stackView.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor,-HorizontalMargin),
                stackView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor,-VerticalMargin)
            });
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                stackView?.RemoveFromSuperview();
                foreach (var v in stackView.ArrangedSubviews)
                    v.RemoveFromSuperview();
                stackView = null;
            }
        }

        public override void RefreshView()
        {
            if (Document == null)
                return;

            stackView.ArrangedSubviews.ForEach(v => v.RemoveFromSuperview());

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
