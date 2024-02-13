using System;
using System.Threading.Tasks;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.ViewControllers.ContactsList;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers.FoldersList
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

            tcs?.TrySetResult(null);
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
            if (!tcs.TrySetResult(null))
                CommonConfig.Logger.Error("Result was already set!");
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
                tcs.TrySetResult(result);

            NavigationController.PopViewController(true);
        }

        protected override async Task FolderExpand(Folder folder)
        {
            await base.FolderExpand(folder);

            var vc = new PickerContactsFoldersListViewController(folder);

            NavigationController.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
                tcs.TrySetResult(result);

            NavigationController.PopViewController(true); 

        }
    }
}
