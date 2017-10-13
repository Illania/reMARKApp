using System;
using System.Collections.Generic;
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
    public class PickerParentContactListViewController : AbstractContactsListViewController
    {
        readonly TaskCompletionSource<ContactPreview> tcs = new TaskCompletionSource<ContactPreview>();
        public Task<ContactPreview> Result => tcs.Task;

        public ContactType ChildrenType { get; set; }

        public PickerParentContactListViewController()
            : base(true)
        {
        }

        public override void Recycle()
        {
            base.Recycle();

            if (!tcs.Task.IsCompleted)
                tcs.SetResult(null);
        }

        protected async override void ContactSelected(UITableView tableView, NSIndexPath indexPath, ContactPreview contactPreview)
        {
            ContactPreview selectedContactPreview = null;

            if (contactPreview.Type == ContactType.Person)
            {
                var content = ChildrenType == ContactType.Person
                                                         ? Localization.GetString("parent_contact_selector_invalid_person_content")
                                                         : Localization.GetString("parent_contact_selector_invalid_department_content");

                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("parent_contact_selector_invalid_person_title"), content);
            }
            else if (contactPreview.Type == ContactType.Department)
            {
                if (ChildrenType == ContactType.Department)
                    await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("parent_contact_selector_invalid_department_title"), Localization.GetString("parent_contact_selector_invalid_department_content"));
                else
                    selectedContactPreview = contactPreview;
            }
            else if (contactPreview.Type == ContactType.Company)
            {
                if (ChildrenType == ContactType.Department)
                    selectedContactPreview = contactPreview;
                else
                {
                    var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("please_wait___"));

                    try
                    {
                        var contact = await Managers.ContactsManager.GetContactAsync(Folder, contactPreview.Id);
                        dismissAction();

                        var deparments = contact.Children.Where(c => c.Type == ContactType.Department);

                        if (!deparments.Any())
                            selectedContactPreview = contactPreview;
                        else
                        {
                            var choices = new List<ContactPreview> { contactPreview };
                            choices.AddRange(deparments);

                            var index = await Dialogs.ShowListActionSheetAsync(this, choices.Select(DisplayText).ToArray(), tableView, tableView.CellAt(indexPath));
                            if (index >= 0)
                                selectedContactPreview = choices[index];
                        }
                    }
                    catch (Exception ex)
                    {
                        dismissAction();
                        CommonConfig.Logger.Error($"Error while retrieving contact [FolderId = {Folder?.Id}, ContactId = {contactPreview.Id}]");
                        await Dialogs.ShowErrorAlertAsync(this, ex);
                    }
                }
            }

            if (selectedContactPreview != null)
            {
                tcs.SetResult(selectedContactPreview);
                NavigationController.PopViewController(false);
            }
        }

        string DisplayText(ContactPreview cp)
        {
            var prefix = cp.Type == ContactType.Company ? Localization.GetString("company") : Localization.GetString("department");
            return $"{prefix}: {cp.Name}";
        }
    }
}
