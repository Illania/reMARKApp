using Mark5.Mobile.IOS.Ui.ViewControllers;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class NavigationController : UINavigationController, ITaggedViewController
    {
        public string Tag { get; set; }

        int searchButtonCounter;

        public NavigationController()
        {
        }

        public NavigationController(UIViewController rootViewController)
            : base(rootViewController)
        {
        }

        public NavigationController(UIViewController rootViewController, UIModalPresentationStyle style)
            : this(rootViewController)
        {
            ModalPresentationStyle = style;
        }

        public NavigationController(UIViewController rootViewController, UIModalPresentationStyle iPhoneStyle, UIModalPresentationStyle iPadStyle)
            : this(rootViewController)
        {
            ModalPresentationStyle = Integration.IsIPad() ? iPadStyle : iPhoneStyle;
        }

        public override void LoadView()
        {
            base.LoadView();
            View.BackgroundColor = Theme.White;
        }

        public override void SetToolbarHidden(bool hidden, bool animated)
        {
            base.SetToolbarHidden(hidden, animated);

            if (SplitViewController == null)
            {
                var del = UIApplication.SharedApplication?.Delegate as AppDelegate;
                var root = del?.Window?.RootViewController as AbstractMainViewController;
                root?.SetSearchButtonHidden(hidden, true);
            }
        }
    }
}