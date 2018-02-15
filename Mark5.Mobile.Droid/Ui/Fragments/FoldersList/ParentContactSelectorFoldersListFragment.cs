using Android.OS;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ParentContactSelectorFoldersListFragment : FoldersListFragment
    {
        const string ChildrenTypeBundleKey = "ChildrenType_5639bddf-ee21-4baf-b5f5-8e1ea19da97c";

        ContactType childrenType;

        public static (ParentContactSelectorFoldersListFragment fragment, string tag) NewInstance(ContactType childrenType, Folder remoteFolder, bool? hideSearch, bool? hideFab, bool? loadRemoteFromCache)
        {
            var args = new Bundle();

            if (childrenType != ContactType.None)
                args.PutInt(ChildrenTypeBundleKey, (int)childrenType);

            if (remoteFolder != null)
                args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder));

            if (hideSearch != null)
                args.PutBoolean(HideSearchBundleKey, hideSearch.Value);

            if (hideFab != null)
                args.PutBoolean(HideFabBundleKey, hideFab.Value);

            if (loadRemoteFromCache != null)
                args.PutBoolean(LoadRemoteFromCacheBundleKey, loadRemoteFromCache.Value);

            ParentContactSelectorFoldersListFragment fragment = new ParentContactSelectorFoldersListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(PickerContactFolderListActivity)}";

            return (fragment, tag);
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(ChildrenTypeBundleKey))
                childrenType = (ContactType)Arguments.GetInt(ChildrenTypeBundleKey);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        protected override void Adapter_ItemClicked(object sender, int position)
        {
            var folder = CurrentAdapter.GetItemAtPosition(position).Folder;
            Activity.StartActivityForResult(ParentContactSelectorActivity.CreateIntent(Context, folder, childrenType), ParentContactSelectorFoldersListActivity.ContactRequestCode);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
            //Nothing to do here
        }

        protected override (BaseFragment fragment, string tag) GetFolderFragment(Folder folder)
        {
            return NewInstance(childrenType, folder, true, true, true);
        }
    }
}
