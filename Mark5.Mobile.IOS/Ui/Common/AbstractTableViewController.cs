using System;
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

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (IsBeingDismissed
                || IsMovingFromParentViewController
                || (NavigationController?.IsBeingDismissed ?? false)
                || (NavigationController?.IsMovingFromParentViewController ?? false))
                Recycle();
        }

        public virtual void Recycle()
        {
        }
    }
}