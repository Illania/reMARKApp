using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class TransitionViewController : UIViewController
    {
        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            var vc = new SplitMainViewController { ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve };
            var window = ((AppDelegate)UIApplication.SharedApplication.Delegate).Window;
            UIView.TransitionNotify(window, 0.25, UIViewAnimationOptions.TransitionCrossDissolve, () => window.RootViewController = vc, null);
        }
    }
}
