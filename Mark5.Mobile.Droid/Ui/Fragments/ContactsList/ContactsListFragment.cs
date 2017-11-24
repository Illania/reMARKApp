using System.Collections.Generic;
using System.Linq;
using Android.Support.Design.Widget;
using Android.Views;
using Android.OS;
using Mark5.Mobile.Common;
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

        public static (ContactsListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new ContactsListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ContactsListFragment)} [folder.id={folder.Id}, folder.name={folder.Name}]";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed)
            {
                fab = ((BaseAppCompatActivity)Activity).Fab;
                fab.SetImageResource(Resource.Drawable.action_add);
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
                StartActivity(ContactActivity.CreateIntent(Context, folder: Folder, contactPreview: contactPreview));
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

            var index = await Dialogs.ShowListDialog(Context, Resource.String.edit_contact_dialog_title, values.Select(v => GetString(UI.ContactTypeResourceId(v))).ToArray(), true);

            if (index > 0)
                StartActivity(AddEditContactActivity.CreateIntent(Context, contactCreationModeFlag: (int)ContactCreationModeFlag.New, contactType: (int)values[index]));

        }
    }
}
