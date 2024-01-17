using System;
using UIKit;

namespace reMark.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells
{
    abstract public class MultiRowContentTableViewCell : AddEditTableViewCell
    {
        protected MultiRowContentTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            : base(style, reuseIdentifier)
        {
        }
    }
}
