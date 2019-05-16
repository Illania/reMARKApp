using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SplitMainViewController : AbstractMainViewController
    {

        DocumentsSplitViewController documentSplitViewController;
        ContactsSplitViewController contactSplitViewController;
        ShortcodesSplitViewController shortcodeSplitViewController;

        NavigationController calendarNavigationController;

        CalendarModuleCoordinator calendarCoordinator;

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

            calendarCoordinator = new CalendarModuleCoordinator();
            calendarNavigationController = calendarCoordinator.RootController;
            calendarNavigationController.RestorationIdentifier = nameof(calendarCoordinator);

            ViewControllers = new UIViewController[]
            {
                SearchNavigationController,
                documentSplitViewController,
                contactSplitViewController,
                calendarNavigationController,
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