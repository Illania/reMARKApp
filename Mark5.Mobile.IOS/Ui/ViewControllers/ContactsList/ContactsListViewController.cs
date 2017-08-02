using Foundation;
using Mark5.Mobile.Common.Model;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ContactsList
{
    public class ContactsListViewController : AbstractContactsListViewController
    {
        public ContactsListViewController()
            : base(false)
        {
        }

        public override void ContactSelected(UITableView tableView, ContactPreview contactPreview, NSIndexPath indexPath)
        {
            if (tableView == SearchResultsController.TableView)
            {
                tableView.SelectRow(indexPath, false, UITableViewScrollPosition.Middle);
            }

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ContactViewController)nc.ViewControllers[0];

                if (vc.IsShowingContactWithId(contactPreview.Id))
                    return;

                vc.ClearData();
                vc.SetData(Folder, contactPreview);
                vc.RefreshData();
            }
            else
            {
                var vc = new ContactViewController();
                vc.SetData(Folder, contactPreview);
                vc.SetRefreshDataOnAppear();
                NavigationController.PushViewController(vc, true);
            }
        }
    }
}