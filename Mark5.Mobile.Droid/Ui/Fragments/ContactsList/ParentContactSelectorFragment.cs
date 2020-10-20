using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.OS;
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
        ContactType childrenType;

        const string ChildrenTypeBundleKey = "ChildrenTypeBundleKey_986c3788-3b4a-445c-8dcd-50f98c738f76";

        Action dismissAction;

        public static (ParentContactSelectorFragment fragment, string tag) NewInstance(ContactType childrenType, Folder folder)
        {
            var args = new Bundle();

            if (childrenType != ContactType.None)
                args.PutInt(ChildrenTypeBundleKey, (int)childrenType);

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            ParentContactSelectorFragment fragment = new ParentContactSelectorFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(AddEditContactFragment)}";

            return (fragment, tag);
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(ChildrenTypeBundleKey))
                childrenType = (ContactType)Arguments.GetInt(ChildrenTypeBundleKey);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
        }

        #region Adapter callbacks

        protected override async void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {
            ContactPreview selectedContactPreview = null;

            if (contactPreview.Type == ContactType.Person)
            {
                var contentResource = childrenType == ContactType.Person ? Resource.String.parent_contact_selector_invalid_person_content : Resource.String.parent_contact_selector_invalid_department_content;

                await Dialogs.ShowConfirmDialogAsync(Activity, Resource.String.parent_contact_selector_invalid_person_title, contentResource);
                return;
            }
            if (contactPreview.Type == ContactType.Department)
            {
                if (childrenType == ContactType.Company || childrenType == ContactType.Department)
                {
                    await Dialogs.ShowConfirmDialogAsync(Activity, Resource.String.parent_contact_selector_invalid_department_title, Resource.String.parent_contact_selector_invalid_department_content);
                    return;
                }

                selectedContactPreview = contactPreview;
            }
            else if (contactPreview.Type == ContactType.Company)
            {
                if (childrenType == ContactType.Department)
                {
                    selectedContactPreview = contactPreview;
                }
                else
                {
                    dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_contact, Resource.String.please_wait);

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
