using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ShortcodesSplitViewController : AbstractSplitViewController
    {
        protected override NavigationController CreatePrimaryNavigationController()
        {
            return new NavigationController(new BrowseFoldersListViewController(ModuleType.Shortcodes))
            {
                RestorationIdentifier = "Primary_NavigationController_" + nameof(FoldersNotificationsListViewController) + "_" + nameof(ModuleType.Shortcodes)
            };
        }

        protected override NavigationController CreateSecondaryNavigationController()
        {
            return new NavigationController(new ShortcodeViewController())
            {
                RestorationIdentifier = "Secondary_NavigationController_" + nameof(ShortcodeViewController)
            };
        }
    }
}