using System;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentPickerFoldersListViewController : AbstractFoldersListViewController
    {
        readonly TaskCompletionSource<Document> tcs = new TaskCompletionSource<Document>();
        public Task<Document> Result => tcs.Task;

        UIBarButtonItem cancelModeItem;

        public DocumentPickerFoldersListViewController()
            : base(ModuleType.Documents, true, false, false, true)
        {
        }

        protected DocumentPickerFoldersListViewController(Folder folder)
            : base(folder, true, false, false, true)
        {
        }

        protected override void InitializeNavigationBar()
        {
            NavigationItem.Title = IsRootOfFoldersList ? Localization.GetString("documents") : ParentFolder.Name;

            if (DisableNavigationBarActions)
                return;

            if (IsRootOfFoldersList)
            {
                cancelModeItem = new UIBarButtonItem
                {
                    Title = Localization.GetString("cancel")
                };
                NavigationItem.SetLeftBarButtonItem(cancelModeItem, false);

                NavigationItem.Title = Localization.GetString("documents");
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

        protected override async void FolderSelected(Folder folder, bool isFromFavorite)
        {
            var vc = new DocumentPickerListViewController
            {
                Folder = folder,
                DisableRowActions = true
            };

            NavigationController?.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
                tcs.SetResult(result);

            DismissViewController(true, null);
        }

        protected override async Task FolderExpand(Folder folder)
        {
            await base.FolderExpand(folder);

            var vc = new DocumentPickerFoldersListViewController(folder);
            NavigationController?.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
                tcs.SetResult(result);

            DismissViewController(true, null);
        }

        void CancelModeItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);
    }
}
