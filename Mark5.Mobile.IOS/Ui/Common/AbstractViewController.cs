using System;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public abstract class AbstractViewController : UIViewController, ITaggedViewController
    {
        public string Tag { get; set; }

        bool recycled;

        public override void LoadView()
        {
            base.LoadView();
            View.BackgroundColor = Theme.White;
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