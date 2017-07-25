using System;

using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public partial class TextFieldTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("TextFieldTableViewCell");
        public static readonly UINib Nib = UINib.FromName("TextFieldTableViewCell", NSBundle.MainBundle);

        protected TextFieldTableViewCell(IntPtr handle) : base(handle)
        {
        }

        public static TextFieldTableViewCell Create()
        {
            var cell = (TextFieldTableViewCell)Nib.Instantiate(null, null)[0];
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
