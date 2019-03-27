using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SplitMainViewController : AbstractMainViewController
    {

        DocumentsSplitViewController documentSplitViewController;
        ContactsSplitViewController contactSplitViewController;
        ShortcodesSplitViewController shortcodeSplitViewController;

        public override void LoadView()
        {
            base.LoadView();

            documentSplitViewController = new DocumentsSplitViewController
            {
                RestorationIdentifier = nameof(DocumentsSplitViewController)
            };

            contactSplitViewController = new ContactsSplitViewController
            {
                RestorationIdentifier = nameof(ContactsSplitViewController)
            };

            shortcodeSplitViewController = new ShortcodesSplitViewController
            {
                RestorationIdentifier = nameof(ShortcodesSplitViewController)
            };

            ViewControllers = new UIViewController[]
            {
                SearchNavigationController,
                documentSplitViewController,
                contactSplitViewController,
                shortcodeSplitViewController,
                SettingsNavigationController
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(SplitViewController);
        }

    }
}