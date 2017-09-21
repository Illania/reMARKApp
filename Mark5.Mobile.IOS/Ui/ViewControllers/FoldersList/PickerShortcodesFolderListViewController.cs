using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ShortcodesList;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public class PickerShortcodesFolderListViewController : AbstractFoldersListViewController
    {
        readonly TaskCompletionSource<Shortcode> tcs = new TaskCompletionSource<Shortcode>();
        public Task<Shortcode> Task => tcs.Task;

        UIBarButtonItem cancelModeItem;

        public PickerShortcodesFolderListViewController()
            : base(ModuleType.Shortcodes, true, true, true)
        {
        }

        protected PickerShortcodesFolderListViewController(Folder folder)
            : base(folder, true, true, true)
        {
        }

        protected async override void FolderSelected(Folder folder)
        {
            var vc = new PickerShortcodesListViewController()
            {
                Folder = folder,
            };
            NavigationController.PushViewController(vc, true);

            var result = await vc.Task;
            if (result != null)
                tcs.SetResult(result);
        }

        protected override void InitializeNavigationBar()
        {
            if (IsRootOfFoldersList)
            {
                cancelModeItem = new UIBarButtonItem();
                cancelModeItem.Title = Localization.GetString("cancel");
                NavigationItem.SetLeftBarButtonItem(cancelModeItem, false);
            }
        }

        protected override void InitializeHandlers()
        {
            base.InitializeHandlers();

            if (cancelModeItem != null)
                cancelModeItem.Clicked += CancelModeItem_Clicked;
        }

        protected override void DeinitializeHandlers()
        {
            base.DeinitializeHandlers();

            if (cancelModeItem != null)
                cancelModeItem.Clicked -= CancelModeItem_Clicked;
        }

        void CancelModeItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
        }

        protected override async void FolderExpand(Folder folder)
        {
            base.FolderExpand(folder);

            var vc = new PickerShortcodesFolderListViewController(folder);
            NavigationController.PushViewController(vc, true);

            var result = await vc.Task;
            if (result != null)
                tcs.SetResult(result);
        }
    }
}
