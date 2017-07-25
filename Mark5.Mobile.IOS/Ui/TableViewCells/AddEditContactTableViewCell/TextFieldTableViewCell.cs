using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class TextFieldTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("TextFieldTableViewCell");

        UITextField TextField { get; set; }

        public TextFieldTableViewCell() : base(UITableViewCellStyle.Default, Key)
        {
            TextField = new UITextField();
            TextField.TranslatesAutoresizingMaskIntoConstraints = false;

            AddSubview(TextField);

            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.TopMargin, 1f, 0f),
                NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.LeadingMargin, 1f, 8f),
                NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.TrailingMargin, 1f, 0f),
                NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.BottomMargin, 1f, 0f),
            });
        }

        public static TextFieldTableViewCell Create()
        {
            var cell = new TextFieldTableViewCell();
            cell.TextField.Font = Theme.DefaultFont;
            cell.TextField.BorderStyle = UITextBorderStyle.None;

            return cell;
        }

        public void Initialize(string hint)
        {
            TextField.Placeholder = hint;
        }
    }
}
