using reMark.Mobile.IOS.Common.ShareExtension;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class SplitMainViewController : AbstractMainViewController
    {

        DocumentsSplitViewController _documentSplitViewController;
        ContactsSplitViewController _contactSplitViewController;
        ShortcodesSplitViewController _shortcodeSplitViewController;
        NotificationsSplitViewController _notificationsSplitViewController;

        public SplitMainViewController() { }

        public SplitMainViewController(SharingOptions sharingOptions)
        {
            this.SharingOptions = sharingOptions;
            OpenedFromSharingOptions = true;
        }

        public override void LoadView()
        {
            base.LoadView();

            _documentSplitViewController = new DocumentsSplitViewController
            {
                RestorationIdentifier = nameof(DocumentsSplitViewController)
            };

            _contactSplitViewController = new ContactsSplitViewController
            {
                RestorationIdentifier = nameof(ContactsSplitViewController)
            };

            _shortcodeSplitViewController = new ShortcodesSplitViewController
            {
                RestorationIdentifier = nameof(ShortcodesSplitViewController)
            };

            _notificationsSplitViewController = new NotificationsSplitViewController()
            {
                RestorationIdentifier = nameof(NotificationsSplitViewController)
            };

            ViewControllers = new UIViewController[]
            {
                _documentSplitViewController,
                _contactSplitViewController,
                _shortcodeSplitViewController,
                _notificationsSplitViewController
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(SplitViewController);
        }

    }
}
