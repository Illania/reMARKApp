using System;
using CoreFoundation;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells
{
    public class TextFieldTableViewCell : AddEditTableViewCell
    {
        public static readonly NSString Key = new NSString("TextFieldTableViewCell");

        public Action<string> ContentEdited;

        readonly UITextField textField;

        public TextFieldTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            textField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                BorderStyle = UITextBorderStyle.None
            };
            textField.EditingChanged += TextField_EditingChanged;
            textField.AutocorrectionType = UITextAutocorrectionType.No;
            ContentView.AddSubview(textField);

            ContentView.AddConstraints(new[]
            {
                textField.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, VerticalMargin),
                textField.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -VerticalMargin),
                textField.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                textField.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
            });
        }

        void TextField_EditingChanged(object sender, EventArgs e)
        {
            ContentEdited?.Invoke(textField.Text);
        }

        public void SetPlaceholder(string placeholder)
        {
            textField.Placeholder = placeholder;
        }

        public void SetContent(string content)
        {
            textField.Text = content;
        }

        public void SetAutocapitalizationType(UITextAutocapitalizationType type)
        {
            textField.AutocapitalizationType = type;
        }

        public void SetAutocorrectionType(UITextAutocorrectionType type)
        {
            textField.AutocorrectionType = type;
        }

        public override void Reset()
        {
            ContentEdited = delegate { };
            SetErrorState(false);

            textField.Placeholder = string.Empty;
            textField.Text = string.Empty;
        }
    }
}
