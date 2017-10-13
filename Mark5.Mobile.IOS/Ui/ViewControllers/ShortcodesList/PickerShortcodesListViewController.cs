using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;
using Foundation;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ShortcodesList
{
    public class PickerShortcodesListViewController : AbstractShortcodesListViewController
    {
        readonly TaskCompletionSource<Shortcode> tcs = new TaskCompletionSource<Shortcode>();
        public Task<Shortcode> Result => tcs.Task;

        public PickerShortcodesListViewController()
            : base(true)
        {
        }

        public async override void ShortcodeSelected(UITableView tableView, NSIndexPath indexPath, ShortcodePreview shortcodePreview)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_shortcode___"));

            try
            {
                var shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(Folder, shortcodePreview.Id);
                dismissAction();

                tcs.SetResult(shortcode);

            }
            catch (Exception ex)
            {
                dismissAction();
                CommonConfig.Logger.Error($"Error while retrieving shortcode [FolderId = {Folder?.Id}, ShortcodeId = {shortcodePreview.Id}]");
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }
        }
    }
}
