using System;
using System.Threading.Tasks;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers.ShortcodesList
{
    public class PickerShortcodesListViewController : AbstractShortcodesListViewController
    {
        readonly TaskCompletionSource<Shortcode> tcs = new TaskCompletionSource<Shortcode>();
        public Task<Shortcode> Result => tcs.Task;

        public PickerShortcodesListViewController()
            : base(true)
        {
        }

        protected override void Recycle()
        {
            base.Recycle();

            if (!tcs.Task.IsCompleted)
                tcs.SetResult(null);
        }

        public async override void ShortcodeSelected(UITableView tableView, NSIndexPath indexPath, ShortcodePreview shortcodePreview)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_shortcode___"));

            try
            {
                var shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(Folder, shortcodePreview.Id);
                dismissAction();
                tcs.SetResult(shortcode);
                DisableSearchController();
                DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                dismissAction();
                CommonConfig.Logger.Error($"Error while retrieving shortcode [FolderId = {Folder?.Id}, ShortcodeId = {shortcodePreview.Id}]");
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }
        }
    }
}
