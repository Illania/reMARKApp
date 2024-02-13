using System;
using reMark.Mobile.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public abstract class AbstractViewController : UIViewController, ITaggedViewController
    {
        public string Tag { get; set; }

        bool recyclingOnDisappearEnabled = true;
        bool recycled;

        public override void LoadView()
        {
            base.LoadView();
            View.BackgroundColor = Theme.White;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (!recyclingOnDisappearEnabled)
                return;

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

        public void DisableRecyclingOnDisappear()
        {
            recyclingOnDisappearEnabled = false;
        }

        public bool IsRecycled() => recycled;

        protected virtual void Recycle()
        {
        }

    }
}