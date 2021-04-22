using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using UIKit;
using Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews;
using System.Collections.Generic;
using Foundation;
using Mark5.Mobile.IOS.Common.ShareExtension;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SimpleMainViewController : AbstractMainViewController
    {
        NavigationController documentsNavigationController;
        NavigationController contactsNavigationController;
        NavigationController shortcodesNavigationController;
        NavigationController calendarNavigationController;

        CalendarModuleCoordinator calendarCoordinator;


        public SimpleMainViewController() { }

        public SimpleMainViewController(SharingOptions sharingOptions)
        {
            this.sharingOptions = sharingOptions;
            openedfromSharingOptions = true;
        }

        public override void LoadView()
        {
            base.LoadView();

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

            calendarCoordinator = new CalendarModuleCoordinator();
            calendarNavigationController = calendarCoordinator.RootController;
            calendarNavigationController.RestorationIdentifier = nameof(calendarCoordinator.RootController) + "_" + nameof(ModuleType.Calendar);

            ViewControllers = new UIViewController[]
            {
                documentsNavigationController,
                contactsNavigationController,
                calendarNavigationController,
                shortcodesNavigationController,
                SettingsNavigationController,
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(SimpleMainViewController);
        }
    }
}