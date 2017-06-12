using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickerContactFolderListFragment : FoldersListFragment
    {
        protected override async void Adapter_ItemClicked(object sender, int position)
        {
            //TODO to coplete
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
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
