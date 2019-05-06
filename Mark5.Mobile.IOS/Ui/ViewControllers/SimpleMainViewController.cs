using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using UIKit;
using Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SimpleMainViewController : AbstractMainViewController
    {
        NavigationController documentsNavigationController;
        NavigationController contactsNavigationController;
        NavigationController shortcodesNavigationController;
        NavigationController calendarNavigationController;

        CalendarCoordinator calendarCoordinator;

        public override void LoadView()
        {
            base.LoadView();

            calendarCoordinator = new CalendarCoordinator();

            contactsNavigationController = new NavigationController(new BrowseFoldersListViewController(ModuleType.Contacts))
            {
                RestorationIdentifier = "NavigationController_" + nameof(BrowseFoldersListViewController) + "_" + nameof(ModuleType.Contacts)
            };

            shortcodesNavigationController = new NavigationController(new BrowseFoldersListViewController(ModuleType.Shortcodes))
            {
                RestorationIdentifier = "NavigationController_" + nameof(BrowseFoldersListViewController) + "_" + nameof(ModuleType.Shortcodes)
            };

            documentsNavigationController = new NavigationController(new FoldersNotificationsListViewController(ModuleType.Documents))
            {
                RestorationIdentifier = "NavigationController_" + nameof(FoldersNotificationsListViewController) + "_" + nameof(ModuleType.Documents)
            };

            calendarNavigationController = calendarCoordinator.NavigationController;

            ViewControllers = new UIViewController[]
            {
                SearchNavigationController,
                documentsNavigationController,
                contactsNavigationController,
                shortcodesNavigationController,
                SettingsNavigationController,
                calendarNavigationController,
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(SimpleMainViewController);
        }
    }
}