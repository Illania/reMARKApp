using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace reMark.Mobile.IOS.Utilities
{
    public static class UITableViewCellUtilities
    {

        public static UITableViewCell CreateDefault(string id, UITableViewCellSelectionStyle selectionStyle = UITableViewCellSelectionStyle.Default)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Default, id);
            cell.TextLabel.Font = Theme.DefaultFont.CustomFont();
            cell.SelectionStyle = selectionStyle;
            return cell;
        }

        public static UITableViewCell CreateWithSubtitle(string id, UITableViewCellSelectionStyle selectionStyle = UITableViewCellSelectionStyle.Default)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, id);
            cell.TextLabel.Font = Theme.DefaultFont.CustomFont();
            cell.DetailTextLabel.Font = Theme.DefaultLightFont.CustomFont().WithRelativeSize(-2f);
            cell.DetailTextLabel.TextColor = Theme.DarkGray;
            cell.SelectionStyle = selectionStyle;
            return cell;
        }

        public static UITableViewCell CreateWithSideText(string id, UITableViewCellSelectionStyle selectionStyle = UITableViewCellSelectionStyle.Default)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Value1, id);
            cell.TextLabel.Font = Theme.DefaultFont.CustomFont();
            cell.DetailTextLabel.Font = Theme.DefaultLightFont.CustomFont().WithRelativeSize(-2f);
            cell.DetailTextLabel.TextColor = Theme.DarkGray;
            cell.SelectionStyle = selectionStyle;
            return cell;
        }
    }
}