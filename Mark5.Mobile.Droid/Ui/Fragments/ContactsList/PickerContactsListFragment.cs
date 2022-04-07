using System;
using System.Linq;
using Android.Content;
using Android.OS;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{ 

    public class PickerContactsListFragment : AbstractContactsListFragment
    {
        Action dismissAction;

        public static (PickerContactsListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new PickerContactsListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ContactsListFragment)} [folder.id={folder.Id}, folder.name={folder.Name}]";

            return (fragment, tag);
        }

        public override void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
        }

        #region Adapter callbacks

        protected override async void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {

           
        }

        protected override void Adapter_ItemLongClicked(object sender, ContactPreview contactPreview)
        {
            //Nothing to do here
        }

        #endregion
    }
}
