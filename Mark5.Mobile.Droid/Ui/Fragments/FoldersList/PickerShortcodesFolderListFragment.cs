using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickerShortcodesFolderListFragment : FoldersListFragment
    {
        public PickerShortcodesFolderListFragment()
            : base(true, true)
        {
            RemoteFolder = Folder.RootForModule(ModuleType.Shortcodes);
        }

        protected override void Adapter_ItemClicked(object sender, int position)
        {
            var folder = CurrentAdapter.GetItemAtPosition(position);
            Activity.StartActivityForResult(PickerShortcodesListActivity.Create(Context, folder), PickerShortcodesFolderListActivity.ShortcodesRequestCode);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
            //Nothing to do here
        }

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new PickerShortcodesFolderListFragment
            {
                RemoteFolder = folder,
            };
        }
    }
}
