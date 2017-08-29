using Android.Content;
using Android.OS;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickerContactFolderListFragment : FoldersListFragment
    {
        public static new (PickerContactFolderListFragment fragment, string tag) NewInstance(Folder remoteFolder, bool? hideSearch = null)
        {
            var tag = $"{nameof(FoldersListFragment)} [FolderId={remoteFolder.Id}, ModuleType={remoteFolder.Module}]";

            var args = new Bundle();

            if(remoteFolder != null)
                args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder));

            if (hideSearch != null)
                args.PutBoolean(HideSearchBundleKey, hideSearch.Value);

            var fragment = new PickerContactFolderListFragment();
            fragment.Arguments = args;

            return (fragment,tag);
        }

        protected override void Adapter_ItemClicked(object sender, int position)
        {
            var folder = CurrentAdapter.GetItemAtPosition(position);
            Activity.StartActivityForResult(PickerContactsListActivity.CreateIntent(Context, folder), PickerContactFolderListActivity.ContactRequestCode);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
            //Nothing to do here
        }

        protected override (RetainableStateFragment fragment, string tag) GetFolderFragment(Folder folder)
        {
            return NewInstance(folder);
        }
    }
}
