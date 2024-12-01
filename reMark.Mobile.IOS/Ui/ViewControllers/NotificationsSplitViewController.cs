using reMark.Mobile.Common.Extensions;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class NotificationsSplitViewController : AbstractSplitViewController
    {
        protected  override NavigationController CreatePrimaryNavigationController()
        {
            return new NavigationController(new NotificationsListViewController(ModuleType.Documents.ObjectTypes()))
            {
                RestorationIdentifier = "Primary_NavigationController_" + nameof(NotificationsListViewController) + "_" + nameof(ModuleType.Documents)
            };
        }

        protected  override NavigationController CreateSecondaryNavigationController()
        {
            return new NavigationController(new NotificationPageViewController())
            {
                RestorationIdentifier = "Secondary_NavigationController_" + nameof(NotificationPageViewController)
            };
        }
    }
}
