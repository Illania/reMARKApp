using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class ShortcodesTableViewCell : UITableViewCell
    {
        public const float Height = 50f;

        public static readonly UINib Nib = UINib.FromName("ShortcodesTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("ShortcodesTableViewCell");

        public ShortcodesTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static ShortcodesTableViewCell Create()
        {
            var cell = (ShortcodesTableViewCell) Nib.Instantiate(null, null)[0];
            cell.NameLabel.Font = Theme.DefaultFont;
            return cell;
        }

        public void Initialize(ShortcodePreview shortcodePreview)
        {
            NameLabel.Text = shortcodePreview.Name;
        }
    }
}