using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ContactsSplitViewController : AbstractSplitViewController
    {
        protected override NavigationController CreatePrimaryNavigationController()
        {
            return new NavigationController(new BrowseFoldersListViewController(ModuleType.Contacts))
            {
                RestorationIdentifier = "Primary_NavigationController_" + nameof(FoldersNotificationsListViewController) + "_" + nameof(ModuleType.Contacts)
            };
        }

        protected override NavigationController CreateSecondaryNavigationController()
        {
            return new NavigationController(new ContactViewController())
            {
                RestorationIdentifier = "Secondary_NavigationController_" + nameof(ContactViewController)
            };
        }
    }
}