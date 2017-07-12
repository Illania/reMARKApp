using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ParentContactSelectorFoldersListFragment : FoldersListFragment
    {
        ContactType childrenType;

        public ParentContactSelectorFoldersListFragment(ContactType childrenType)
            : base(true, true)
        {
            RemoteFolder = Folder.RootForModule(ModuleType.Contacts);
            this.childrenType = childrenType;
        }

        protected override void Adapter_ItemClicked(object sender, int position)
        {
            var folder = CurrentAdapter.GetItemAtPosition(position);
            Activity.StartActivityForResult(ParentContactSelectorActivity.Create(Context, folder, childrenType), ParentContactSelectorFoldersListActivity.ContactRequestCode);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
            //Nothing to do here
        }

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new ParentContactSelectorFoldersListFragment(childrenType)
            {
                RemoteFolder = folder,
            };
        }
    }
}
