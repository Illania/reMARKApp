using System;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    abstract public class MultiRowContentTableViewCell : AddEditContactTableViewCell
    {
        protected MultiRowContentTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            : base(style, reuseIdentifier)
        {
        }

        abstract public void StartEditing();
    }
}
