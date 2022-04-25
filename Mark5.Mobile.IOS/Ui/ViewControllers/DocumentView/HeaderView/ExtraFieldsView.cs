using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
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
                var stackViewHorizontal = new UIStackView
                {
                    Opaque = false,
                    Axis = UILayoutConstraintAxis.Horizontal,
                    Alignment = UIStackViewAlignment.FirstBaseline,
                    Distribution = UIStackViewDistribution.Fill,
                    Spacing = InnerMargin,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                var labelText = new NSMutableAttributedString(efi.Key.Name + ": ");
                var valueText = new NSMutableAttributedString(efi.Value);
                labelText.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.DarkGray, new NSRange(0, efi.Key.Name.Length + 1));
  

                var label = new UILabelScalable
                {
                    Font = Theme.DefaultFont.CustomFont(),
                    Opaque = false,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Tag = efi.Key.Id,
                    
                };

                label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                label.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

                var valueTextField = new UITextFieldScalable
                {
                    Font = Theme.DefaultFont.CustomFont(),
                    Opaque = false,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Tag = efi.Key.Id,
                    TextAlignment = UITextAlignment.Left
                };
                valueTextField.EditingDidEnd += ValueTextField_EditingDidEnd; ;
                
                label.AttributedText = labelText;
                valueTextField.AttributedText = valueText;

                stackViewHorizontal.AddArrangedSubview(label);
                stackViewHorizontal.AddArrangedSubview(valueTextField);
                stackView.AddArrangedSubview(stackViewHorizontal);
            }
        }

        private async void ValueTextField_EditingDidEnd(object sender, System.EventArgs e)
        {
            var fieldValue = ((UITextFieldScalable)sender).Text;
            var fieldId = ((UITextFieldScalable)sender).Tag;
            var docId = Document.Id;
            await Managers.DocumentsManager.AssignDocumentExtraFieldAsync(docId, (int)fieldId, fieldValue); 

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
