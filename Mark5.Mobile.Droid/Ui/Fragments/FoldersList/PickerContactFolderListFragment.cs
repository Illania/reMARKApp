using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickerContactFolderListFragment : FoldersListFragment
    {
        public PickerContactFolderListFragment()
        {
            RemoteFolder = Folder.RootForModule(ModuleType.Contacts);
            HideSearch = true;
            HideFab = true;
            LoadRemoteFromCache = true;
        }

        protected override void Adapter_ItemClicked(object sender, int position)
        {
            var folder = CurrentAdapter.GetItemAtPosition(position);
            Activity.StartActivityForResult(PickerContactsListActivity.Create(Context, folder), PickerContactFolderListActivity.ContactRequestCode);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
            //Nothing to do here
        }

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new PickerContactFolderListFragment
            {
                RemoteFolder = folder,
            };
        }
    }
}