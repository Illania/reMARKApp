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
        public static ShortcodesListFragment NewInstance(Folder folder)
        {
            var args = new Bundle();
            args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new ShortcodesListFragment();
            fragment.Arguments = args;

            return fragment;
        }

        #region Adapter callbacks

        protected override void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (ActionMode == null)
            {
                StartActivity(ShortcodeActivity.CreateIntent(Activity, folder: Folder, shortcodePreview: shortcodePreview));
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
