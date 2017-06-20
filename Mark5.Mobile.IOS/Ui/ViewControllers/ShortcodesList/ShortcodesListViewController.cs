using Mark5.Mobile.Common.Model;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ShortcodesList
{
    public class ShortcodesListViewController : AbstractShortcodesListViewController
    {
        public ShortcodesListViewController()
            : base(false)
        {
        }

        public override void ShortcodeSelected(UITableView tableView, ShortcodePreview shortcodePreview)
        {
            if (tableView == SearchResultsController.TableView)
            {
                var ds = (DataSource) tableView.Source;
                var indexPath = ds.FindItemIndexPath(shortcodePreview);
                if (indexPath != null)
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.Middle);
            }

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController) nc.ViewControllers[0];

                if (vc.IsShowingShortcodeWithId(shortcodePreview.Id))
                    return;

                vc.ClearData();
                vc.SetData(Folder, shortcodePreview);
                vc.RefreshData();
            }
            else
            {
                var vc = new ShortcodeViewController();
                vc.SetData(Folder, shortcodePreview);
                vc.SetRefreshDataOnAppear();
                NavigationController.PushViewController(vc, true);
            }
        }
    }
}
