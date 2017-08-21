using System;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    abstract public class MultiRowContentTableViewCell : AddEditTableViewCell
    {
        protected MultiRowContentTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            : base(style, reuseIdentifier)
        {
        }
    }
}
