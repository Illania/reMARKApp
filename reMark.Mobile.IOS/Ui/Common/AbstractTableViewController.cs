using System;
using reMark.Mobile.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public class AbstractTableViewController : UITableViewController, ITaggedViewController
    {
        public string Tag { get; set; }

        bool recycled;

        public AbstractTableViewController()
        {
        }

        public AbstractTableViewController(UITableViewStyle withStyle)
            : base(withStyle)
        {
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (IsBeingDismissed
                || IsMovingFromParentViewController
                || (NavigationController?.IsBeingDismissed ?? false)
                || (NavigationController?.IsMovingFromParentViewController ?? false))
            {
                RecycleIfNeeded();
#if DEBUG
                GC.Collect();
#endif
            }
        }

        public void RecycleIfNeeded()
        {
            if (!recycled)
            {
                Recycle();
                recycled = true;
            }
        }

        protected virtual void Recycle()
        {
        }
    }
}