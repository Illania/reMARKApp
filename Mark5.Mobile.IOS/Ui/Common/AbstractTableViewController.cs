using System;
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
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

            if (SplitViewController == null)
                CommonConfig.UsageAnalytics.SetScreen(GetType().Name);
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