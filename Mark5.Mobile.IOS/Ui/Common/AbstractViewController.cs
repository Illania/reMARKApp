using System;
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
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

            if (SplitViewController == null && !(this is AbstractMultiViewController))
                CommonConfig.UsageAnalytics.SetScreen(GetType().Name);
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

        protected virtual void Recycle()
        {
        }
    }
}