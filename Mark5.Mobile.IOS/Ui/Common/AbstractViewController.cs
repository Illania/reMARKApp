using UIKit;
using System;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public abstract class AbstractViewController : UIViewController, ITaggedViewController
    {
        public string Tag { get; set; }

        public override void LoadView()
        {
            base.LoadView();
            View.BackgroundColor = UIColor.White;
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