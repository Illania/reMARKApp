using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.ViewControllers.FoldersList;
using UIKit;
using reMark.Mobile.IOS.Common.ShareExtension;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class SimpleMainViewController : AbstractMainViewController
    {
        NavigationController documentsNavigationController;
        NavigationController contactsNavigationController;
        NavigationController shortcodesNavigationController;


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

            ViewControllers = new UIViewController[]
            {
                documentsNavigationController,
                contactsNavigationController,
                shortcodesNavigationController,
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(SimpleMainViewController);
        }
    }
}