using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.IOS.Ui.Common;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{

    public class ExtraFieldsView : DocumentSubView
    {

        UIStackView stackView;
        Dictionary<DocumentExtraFieldInfo, string> documentExtraFields = new();
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

      
        public override async Task RefreshView()
        {
            if (Document == null)
                return;

            stackView.ArrangedSubviews.ForEach(v => v.RemoveFromSuperview());

            if (ServerConfig.SystemSettings.SystemInfo.ExtraFieldsEditingAvailable)
                AddEditableExtraFields();
            else
                AddReadonlyExtraFields();
        }

        private void AddEditableExtraFields()
        {
            DocumentExtraFieldInfoEqualityComparer docComparer = new();

            var assignedExtraFields = Document.ExtraFields;
            var availableExtraFields = ServerConfig.SystemSettings.DocumentsModuleInfo.ExtraFieldInfos;
            availableExtraFields = availableExtraFields.Where(ef => PlatformConfig.Preferences.IsExtraFieldEnabled(ef.Id)).ToList();
            documentExtraFields = assignedExtraFields
                .Where(kv => kv.Key != null)
                .OrderBy(kv => kv.Key.Name)
                .ToDictionary(pair => pair.Key, pair => pair.Value, docComparer);

            foreach (var ex in availableExtraFields)
            {
                if (!documentExtraFields.ContainsKey(ex))
                    documentExtraFields.Add(ex, string.Empty);
            }

            if (Document != null)
            {
                foreach (var efi in documentExtraFields)
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
                    valueTextField.EditingDidEnd += ValueTextField_EditingDidEnd;

                    valueTextField.UserInteractionEnabled = true;

                    label.AttributedText = labelText;
                    valueTextField.AttributedText = valueText;

                    stackViewHorizontal.AddArrangedSubview(label);
                    stackViewHorizontal.AddArrangedSubview(valueTextField);
                    stackView.AddArrangedSubview(stackViewHorizontal);
                }
            }
        }

        private void AddReadonlyExtraFields()
        {
            foreach (var efi in Document.ExtraFields)
            {
                var text = new NSMutableAttributedString(efi.Key.Name + ": " + efi.Value);
                text.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.DarkGray, new NSRange(0, efi.Key.Name.Length + 1));
                

                var label = new UILabelScalable
                {
                    Font = Theme.DefaultFont.CustomFont(),
                    Opaque = false,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                label.AttributedText = text;
                stackView.AddArrangedSubview(label);      
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

            Hidden = !documentExtraFields.Any();
        }
    }
}
