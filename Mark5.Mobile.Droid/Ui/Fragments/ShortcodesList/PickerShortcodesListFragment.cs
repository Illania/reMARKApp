using System;
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
    public class PickerShortcodesListFragment : AbstractShortcodesListFragment
    {
        public static (PickerShortcodesListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var tag = $"{nameof(AbstractShortcodesListFragment)} [folder.id={folder.Id}, folder.name={folder.Name}]";

            var args = new Bundle();
            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new PickerShortcodesListFragment();
            fragment.Arguments = args;

            return (fragment, tag);
        }

        #region Adapter callbacks

        protected override async void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_shortcode, Resource.String.please_wait);

            try
            {
                var shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(Folder, shortcodePreview.Id);
                dismissAction();

                var data = new Intent();
                data.PutExtra(PickerShortcodesListActivity.ShortcodeResultKey, Serializer.Serialize(shortcode));
                Activity.SetResult(Android.App.Result.Ok, data);
                Activity?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();
                CommonConfig.Logger.Error($"Error while retrieving shortcode [FolderId = {Folder?.Id}, ShortcodeId = {shortcodePreview.Id}]");
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        protected override void Adapter_ItemLongClicked(object sender, ShortcodePreview shortcodePreview)
        {
            //Nothing to do here
        }

        #endregion
    }
}
