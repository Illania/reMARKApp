using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ParentContactSelectorFragment : AbstractContactsListFragment
    {
        public ContactType ChildrenType { get; set; }

        #region Adapter callbacks

        protected override async void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {
            ContactPreview selectedContactPreview = null;

            if (contactPreview.Type == ContactType.Person)
            {
                await Dialogs.ShowConfirmDialogAsync(Activity, Resource.String.parent_contact_selector_invalid_person_title, Resource.String.parent_contact_selector_invalid_person_content);
                return;
            }
            if (contactPreview.Type == ContactType.Department)
            {
                if (ChildrenType == ContactType.Company || ChildrenType == ContactType.Department)
                {
                    await Dialogs.ShowConfirmDialogAsync(Activity, Resource.String.parent_contact_selector_invalid_department_title, Resource.String.parent_contact_selector_invalid_department_content);
                    return;
                }

                selectedContactPreview = contactPreview;
            }
            else if (contactPreview.Type == ContactType.Company)
            {

                if (ChildrenType == ContactType.Department)
                {
                    selectedContactPreview = contactPreview;
                }
                else
                {
                    var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_contact, Resource.String.please_wait);

                    try
                    {
                        var contact = await Managers.ContactsManager.GetContactAsync(Folder, contactPreview.Id);
                        dismissAction();

                        var deparments = contact.Children.Where(c => c.Type == ContactType.Department);

                        if (!deparments.Any())
                        {
                            selectedContactPreview = contactPreview;
                        }
                        else
                        {
                            var choices = new List<ContactPreview> { contactPreview };
                            choices.AddRange(deparments);

                            var choice = await Dialogs.ShowSingleSelectDialogAsync(Activity, Resource.String.parent_contact_selector_choose, choices, displayText: DisplayText);

                            selectedContactPreview = choice;
                        }
                    }
                    catch (Exception ex)
                    {
                        dismissAction();
                        CommonConfig.Logger.Error($"Error while retrieving contact [FolderId = {Folder?.Id}, ContactId = {contactPreview.Id}]");
                        await Dialogs.ShowErrorDialogAsync(Activity, ex);
                    }
                }
            }

            if (selectedContactPreview != null)
            {
                var data = new Intent();
                data.PutExtra(ParentContactSelectorActivity.ParentContactResultKey, Serializer.Serialize(selectedContactPreview));
                Activity.SetResult(Android.App.Result.Ok, data);
                Activity.Finish();
                return;
            }
        }

        string DisplayText(ContactPreview cp)
        {
            var prefix = cp.Type == ContactType.Company ? Activity.GetString(Resource.String.company) : Activity.GetString(Resource.String.department);
            return $"{prefix}: {cp.Name}";
        }

        protected override void Adapter_ItemLongClicked(object sender, ContactPreview contactPreview)
        {
            //Nothing to do here
        }

        #endregion

    }
}
