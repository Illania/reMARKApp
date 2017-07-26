using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class TextFieldTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("TextFieldTableViewCell");

        public event EventHandler<string> ContentEdited = delegate { };

        UITextField TextField { get; set; }

        public TextFieldTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            TextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                BorderStyle = UITextBorderStyle.None
            };
            TextField.EditingDidEnd += (object sender, EventArgs e) => ContentEdited(this, TextField.Text);

            ContentView.AddSubview(TextField);

            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, 0f),
                NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeadingMargin, 1f, 8f),
                NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TrailingMargin, 1f, 0f),
                NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, 0f),
            });
        }

        public void SetPlaceholder(string placeholder)
        {
            TextField.Placeholder = placeholder;
        }

        public void SetContent(string content)
        {
            TextField.Text = content;
        }
    }
}
