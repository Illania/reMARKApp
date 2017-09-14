using Android.OS;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ContactsListFragment : AbstractContactsListFragment
    {
        const string FolderBundleKey = "Folder_d3ded4d4-be9a-49e6-8626-84cb175c12b4";

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

        #region MyRegion

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(FolderBundleKey))
                Folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        #endregion

        #region Adapter callbacks

        protected override void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {
            if (ActionMode == null)
            {
                StartActivity(ContactActivity.CreateIntent(Context, folder:Folder, contactPreview:contactPreview));
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
    }
}
