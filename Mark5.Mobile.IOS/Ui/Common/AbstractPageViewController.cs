using System;
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public abstract class AbstractPageViewController : UIPageViewController, ITaggedViewController
    {
        public string Tag { get; set; }

        bool recycled;

        protected AbstractPageViewController(UIPageViewControllerTransitionStyle style, UIPageViewControllerNavigationOrientation navigationOrientation, UIPageViewControllerSpineLocation spineLocation)
            : base(style, navigationOrientation, spineLocation)
        {
        }

        public override void LoadView()
        {
            base.LoadView();
            View.BackgroundColor = Theme.White;
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
