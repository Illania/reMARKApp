using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public class ExternalDocumentFoldersListViewController : AbstractFoldersListViewController
    {
        readonly TaskCompletionSource<List<AttachmentDescription>> tcs = new TaskCompletionSource<List<AttachmentDescription>>();
        public Task<List<AttachmentDescription>> Result => tcs.Task;

        UIBarButtonItem cancelModeItem;

        public ExternalDocumentFoldersListViewController()
            : base(ModuleType.Documents, true, false, false, true)
        {
        }

        protected ExternalDocumentFoldersListViewController(Folder folder)
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

                NavigationItem.Title = Localization.GetString("external_documents");
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
            var vc = new ExternalDocumentsListViewController
            {
                Folder = folder,
                DisableRowActions = true,
                OnlyShowExternalDocuments = true
            };

            NavigationController.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
                tcs.SetResult(result);

            DismissViewController(true, null);
        }

        protected override async void FolderExpand(Folder folder)
        {
            var vc = new ExternalDocumentFoldersListViewController(folder);
            NavigationController.PushViewController(vc, true);

            var result = await vc.Result;
            if (result != null)
                tcs.SetResult(result);

            DismissViewController(true, null);
        }

        void CancelModeItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);
    }
}