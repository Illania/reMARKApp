using System;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ContactsList
{
    public class PickerContactsListViewController : AbstractContactsListViewController
    {
        readonly TaskCompletionSource<Recipient> tcs = new TaskCompletionSource<Recipient>();
        public Task<Recipient> Result => tcs.Task;

        public PickerContactsListViewController()
            : base(true)
        {
        }

        protected override void Recycle()
        {
            base.Recycle();

            if (!tcs.Task.IsCompleted)
                tcs.SetResult(null);
        }

        protected async override void ContactSelected(UITableView tableView, NSIndexPath indexPath, ContactPreview contactPreview)
        {
            try
            {
                var vc = new ContactEmailAddressesViewController();
                vc.SetData(Folder, contactPreview);
                NavigationController.PushViewController(vc, true);

                var result = await vc.Result;
                if (result != null)
                {
                    if (!tcs.TrySetResult(result))
                        CommonConfig.Logger.Error("Result was already set!");    
                }
                NavigationController.PopViewController(true); 

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving contact email addresses [FolderId = {Folder?.Id}, ContactId = {contactPreview.Id}]");
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }
        }
    }
}
