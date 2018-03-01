using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentsSplitViewController : AbstractSplitViewController
    {
        protected override NavigationController CreatePrimaryNavigationController()
        {
            return new NavigationController(new FoldersNotificationsListViewController(ModuleType.Documents))
            {
                RestorationIdentifier = "Primary_NavigationController_" + nameof(FoldersNotificationsListViewController) + "_" + nameof(ModuleType.Documents)
            };
        }

        protected override NavigationController CreateSecondaryNavigationController()
        {
            return new NavigationController(new DocumentPageViewController())
            {
                RestorationIdentifier = "Secondary_NavigationController_" + nameof(DocumentPageViewController)
            };
        }
    }
}