using Android.OS;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickerShortcodesFolderListFragment : FoldersListFragment
    {
        public static (PickerShortcodesFolderListFragment fragment, string tag) NewInstance(Folder remoteFolder, bool? hideSearch = null, bool? hideFab = null, bool? loadRemoteFromCache = null)
        {
            var args = new Bundle();

            if (remoteFolder != null)
                args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder));

            if (hideSearch != null)
                args.PutBoolean(HideSearchBundleKey, hideSearch.Value);

            if (hideFab != null)
                args.PutBoolean(HideFabBundleKey, hideFab.Value);

            if (loadRemoteFromCache != null)
                args.PutBoolean(LoadRemoteFromCacheBundleKey, loadRemoteFromCache.Value);

            var fragment = new PickerShortcodesFolderListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(FoldersListFragment)} [FolderId={remoteFolder.Id}, ModuleType={remoteFolder.Module}]";

            return (fragment,tag);
        }

        protected override void Adapter_ItemClicked(object sender, int position)
        {
            var folder = CurrentAdapter.GetItemAtPosition(position);
            Activity.StartActivityForResult(PickerShortcodesListActivity.CreateIntent(Context, folder), PickerShortcodesFolderListActivity.ShortcodesRequestCode);
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