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
        public static new PickerContactFolderListFragment NewInstance(Folder remoteFolder, bool? hideSearch = null)
        {
            var args = new Bundle();
            args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder));

            if (hideSearch != null)
                args.PutBoolean(HideSearchBundleKey, hideSearch.Value);

            var fragment = new PickerContactFolderListFragment();
            fragment.Arguments = args;

            return fragment;
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

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return PickerContactFolderListFragment.NewInstance(folder);
        }
    }
}
