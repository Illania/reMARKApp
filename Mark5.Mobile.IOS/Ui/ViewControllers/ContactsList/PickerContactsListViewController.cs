using System;
using System.Linq;
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

        protected async override void ContactSelected(UITableView tableView, NSIndexPath indexPath, ContactPreview contactPreview)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_contact___"));

            try
            {
                var contact = await Managers.ContactsManager.GetContactAsync(Folder, contactPreview.Id);
                dismissAction();

                var ds = (DataSource)tableView.Source;
                var cell = tableView.CellAt(ds.FindItemIndexPath(contactPreview));

                var emailAddresses = contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Email).Select(ca => ca.Address).ToArray();
                if (emailAddresses.Any())
                {
                    var index = await Dialogs.ShowListDialogAsync(this, null, emailAddresses, cell);
                    if (index < 0)
                        return;

                    var address = emailAddresses[index];

                    tcs.SetResult(new Recipient(contactPreview.Name, address, RecipientType.Contact));
                }
                else
                {
                    await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("no_email_addresses_title"), Localization.GetString("no_email_addresses_content"));
                }
            }
            catch (Exception ex)
            {
                dismissAction();
                CommonConfig.Logger.Error($"Error while retrieving contact [FolderId = {Folder?.Id}, ContactId = {contactPreview.Id}]");
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }
        }
    }
}
