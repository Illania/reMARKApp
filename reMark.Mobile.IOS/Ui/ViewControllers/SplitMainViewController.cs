using reMark.Mobile.IOS.Common.ShareExtension;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class SplitMainViewController : AbstractMainViewController
    {

        DocumentsSplitViewController documentSplitViewController;
        ContactsSplitViewController contactSplitViewController;
        ShortcodesSplitViewController shortcodeSplitViewController;

        public SplitMainViewController() { }

        public SplitMainViewController(SharingOptions sharingOptions)
        {
            this.sharingOptions = sharingOptions;
            openedfromSharingOptions = true;
        }

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