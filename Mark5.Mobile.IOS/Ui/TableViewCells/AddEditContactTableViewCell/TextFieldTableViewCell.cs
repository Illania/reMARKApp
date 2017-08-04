using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class TextFieldTableViewCell : AddEditContactTableViewCell
    {
        public static readonly NSString Key = new NSString("TextFieldTableViewCell");

        public event EventHandler<string> ContentEdited = delegate { };

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
            textField.EditingChanged += (object sender, EventArgs e) => ContentEdited(this, textField.Text);

            ContentView.AddSubview(textField);

            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(textField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(textField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(textField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(textField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
            });
        }

        public void SetPlaceholder(string placeholder)
        {
            textField.Placeholder = placeholder;
        }

        public void SetContent(string content)
        {
            textField.Text = content;
        }
    }
}
