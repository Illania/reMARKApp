using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class AbstractTableViewController : UITableViewController, ITaggedViewController
    {
        public string Tag { get; set; }

        public AbstractTableViewController()
        {
        }

        public AbstractTableViewController(UITableViewStyle withStyle)
            : base(withStyle)
        {
        }
    }
}