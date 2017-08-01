using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ContactsListFragment : AbstractContactsListFragment
    {
        FloatingActionButton fab;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed)
            {
                fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
                fab.SetImageResource(Resource.Drawable.action_add_contact);
                fab.SetOnClickListener(new ActionOnClickListener(CreateContact));
                fab.Visibility = ViewStates.Visible;
            }

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        #region Adapter callbacks

        protected override void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {
            if (ActionMode == null)
            {
                var i = new Intent(Activity, typeof(ContactActivity));
                i.PutExtra(ContactActivity.ContactPreviewIntentKey, Serializer.Serialize(contactPreview));
                i.PutExtra(ContactActivity.FolderIntentKey, Serializer.Serialize(Folder));
                StartActivity(i);
            }
            else
            {
                CurrentAdapter.SetSelected(contactPreview, !CurrentAdapter.IsSelected(contactPreview));

                if (CurrentAdapter.SelectedItemCount < 1)
                {
                    ActionMode.Finish();
                }
                else
                {
                    ActionMode.Title = CurrentAdapter.SelectedItemCount.ToString();
                    ActionMode.Invalidate();
                }
            }
        }

        protected override void Adapter_ItemLongClicked(object sender, ContactPreview contactPreview)
        {
            if (ActionMode == null)
                ActionMode = Activity.StartActionMode(this);

            Adapter_ItemClicked(sender, contactPreview);
        }

        #endregion

        async void CreateContact()
        {
            var values = new List<ContactType> { ContactType.Company, ContactType.Department, ContactType.Person };

            var index = await Dialogs.ShowListDialog(Context, Resource.String.edit_contact_dialog_title, values.Select(v => GetString(UI.ContactTypeResourceId(v))).ToArray(),
                                                                   true);

            if (index >= 0)
            {
                var intent = new Intent(Context, typeof(AddEditContactActivity));
                intent.PutExtra(AddEditContactActivity.ContactCreationModeFlag, (int)ContactCreationModeFlag.New);
                intent.PutExtra(AddEditContactActivity.ContactTypeIntentKey, (int)values[index]);
                StartActivity(intent);
            }
        }
    }
}
