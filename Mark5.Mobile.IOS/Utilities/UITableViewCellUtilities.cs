using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class UITableViewCellUtilities
    {

        public static UITableViewCell CreateDefault(string id, UITableViewCellSelectionStyle selectionStyle = UITableViewCellSelectionStyle.Default)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Default, id);
            cell.TextLabel.Font = Theme.DefaultFont;
            cell.SelectionStyle = selectionStyle;
            return cell;
        }

        public static UITableViewCell CreateWithSubtitle(string id, UITableViewCellSelectionStyle selectionStyle = UITableViewCellSelectionStyle.Default)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, id);
            cell.TextLabel.Font = Theme.DefaultFont;
            cell.DetailTextLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);
            cell.DetailTextLabel.TextColor = Theme.DarkGray;
            cell.SelectionStyle = selectionStyle;
            return cell;
        }

        public static UITableViewCell CreateWithSideText(string id, UITableViewCellSelectionStyle selectionStyle = UITableViewCellSelectionStyle.Default)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Value1, id);
            cell.TextLabel.Font = Theme.DefaultFont;
            cell.DetailTextLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);
            cell.DetailTextLabel.TextColor = Theme.DarkGray;
            cell.SelectionStyle = selectionStyle;
            return cell;
        }
    }
}