using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class ShortcodeInfoTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("ShortcodeInfoTableViewCell");
        public static readonly UINib Nib = UINib.FromName("ShortcodeInfoTableViewCell", NSBundle.MainBundle);

        protected ShortcodeInfoTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static ShortcodeInfoTableViewCell Create()
        {
            var cell = (ShortcodeInfoTableViewCell)Nib.Instantiate(null, null)[0];
            cell.TypeLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-3f);
            cell.InfoTextView.Font = Theme.DefaultFont;
            cell.InfoTextView.Editable = false;
            cell.InfoTextView.TextContainerInset = UIEdgeInsets.Zero;
            cell.InfoTextView.TextContainer.LineFragmentPadding = 0;

            return cell;
        }

        public void Initialize(string type, string info, bool enableDataDetection = false)
        {
            TypeLabel.Text = type;
            InfoTextView.Text = info;

            if (enableDataDetection)
                InfoTextView.DataDetectorTypes = UIDataDetectorType.All;
        }

        public void Initialize(string type, NSAttributedString info)
        {
            TypeLabel.Text = type.ToUpper();
            InfoTextView.AttributedText = info;
        }
    }
}