using System;
using reMark.Mobile.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public abstract class AbstractPageViewController : UIPageViewController, ITaggedViewController
    {
        public string Tag { get; set; }

        bool recycled;

        protected AbstractPageViewController()
            : base(UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal, UIPageViewControllerSpineLocation.None)
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
