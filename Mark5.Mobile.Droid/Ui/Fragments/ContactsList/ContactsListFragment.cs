
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ContactsListFragment : AbstractContactsListFragment
    {
        #region Adapter callbacks

        protected override void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {
            if (ActionMode == null)
            {
                var i = new Intent(Activity, typeof(ContactActivity));
                i.PutExtra(ContactActivity.ContactPreviewIntentKey, Serializer.Serialize(contactPreview));
                i.PutExtra(ContactActivity.FolderIntentKey, Serializer.Serialize(Folder));
                StartActivity(i);
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
