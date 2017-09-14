using System;
using Android.Views;
using Android.OS;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ShortcodesListFragment : AbstractShortcodesListFragment
    {
        public static (ShortcodesListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new ShortcodesListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(AbstractShortcodesListFragment)} [folder.id={folder.Id}, folder.name={folder.Name}]";

            return (fragment, tag);
        }

        #region Adapter callbacks

        protected override void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (ActionMode == null)
            {
                StartActivity(ShortcodeActivity.CreateIntent(Context, folder: Folder, shortcodePreview: shortcodePreview));
            }
            else
            {
                CurrentAdapter.SetSelected(shortcodePreview, !CurrentAdapter.IsSelected(shortcodePreview));

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

        protected override void Adapter_ItemLongClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (ActionMode == null)
                ActionMode = Activity.StartActionMode(this);

            Adapter_ItemClicked(sender, shortcodePreview);
        }

        #endregion
    }
}
