using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ContactsList;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public class PickerContactsFoldersListViewController : AbstractFoldersListViewController
    {
        readonly TaskCompletionSource<Recipient> tcs = new TaskCompletionSource<Recipient>();
        public Task<Recipient> Result => tcs.Task;

        UIBarButtonItem cancelItem;

        public PickerContactsFoldersListViewController()
            : base(ModuleType.Contacts, true, true, true)
        {
        }

        protected PickerContactsFoldersListViewController(Folder folder)
            : base(folder, true, true, true)
        {
        }

        protected override void Recycle()
        {
            base.Recycle();

            if (!tcs.Task.IsCompleted)
                tcs.SetResult(null);
        }

        protected override void InitializeNavigationBar()
        {
            if (IsRootOfFoldersList)
                NavigationItem.Title = Localization.GetString("contacts");
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

        void CancelItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
            tcs.SetResult(null);
        }

        protected async override void FolderSelected(Folder folder, bool isFromFavorite)
        {
            base.FolderSelected(folder, isFromFavorite);

            var vc = new PickerContactsListViewController
            {
                Folder = folder,
            };
            NavigationController.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
                tcs.SetResult(result);
        }

        protected override async void FolderExpand(Folder folder)
        {
            base.FolderExpand(folder);

            var vc = new PickerContactsFoldersListViewController(folder);
            NavigationController.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
                tcs.SetResult(result);
        }
    }
}
