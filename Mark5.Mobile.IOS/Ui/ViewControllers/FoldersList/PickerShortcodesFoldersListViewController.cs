using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ShortcodesList;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public class PickerShortcodesFoldersListViewController : AbstractFoldersListViewController
    {
        readonly TaskCompletionSource<Shortcode> tcs = new TaskCompletionSource<Shortcode>();
        public Task<Shortcode> Result => tcs.Task;

        UIBarButtonItem cancelItem;

        public PickerShortcodesFoldersListViewController()
            : base(ModuleType.Shortcodes, true, true, true)
        {
        }

        protected PickerShortcodesFoldersListViewController(Folder folder)
            : base(folder, true, true, true)
        {
        }

        public override void Recycle()
        {
            base.Recycle();

            if (!tcs.Task.IsCompleted)
                tcs.SetResult(null);
        }

        protected override void InitializeNavigationBar()
        {
            if (IsRootOfFoldersList)
                NavigationItem.Title = Localization.GetString("shortcodes");
            else
                NavigationItem.Title = ParentFolder.Name;

            if (IsRootOfFoldersList)
            {
                cancelItem = new UIBarButtonItem
                {
                    Title = Localization.GetString("cancel")
                };
                NavigationItem.SetLeftBarButtonItem(cancelItem, false);
            }
        }

        protected override void InitializeHandlers()
        {
            base.InitializeHandlers();

            if (cancelItem != null)
                cancelItem.Clicked += CancelItem_Clicked;
        }

        protected override void DeinitializeHandlers()
        {
            base.DeinitializeHandlers();

            if (cancelItem != null)
                cancelItem.Clicked -= CancelItem_Clicked;
        }

        void CancelItem_Clicked(object sender, EventArgs e) => tcs.SetResult(null);

        protected async override void FolderSelected(Folder folder)
        {
            var vc = new PickerShortcodesListViewController
            {
                Folder = folder,
            };
            NavigationController.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
            {
                if (NavigationController.ViewControllers.Length == 1 && NavigationController.ViewControllers[0] == this)
                    NavigationController.DismissViewController(true, null);
                else
                    NavigationController.PopViewController(false);
                tcs.SetResult(result);
            }
        }

        protected override async void FolderExpand(Folder folder)
        {
            base.FolderExpand(folder);

            var vc = new PickerShortcodesFoldersListViewController(folder);
            NavigationController.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
            {
                if (NavigationController.ViewControllers.Length == 1 && NavigationController.ViewControllers[0] == this)
                    NavigationController.DismissViewController(true, null);
                else
                    NavigationController.PopViewController(false);
                tcs.SetResult(result);
            }
        }
    }
}
