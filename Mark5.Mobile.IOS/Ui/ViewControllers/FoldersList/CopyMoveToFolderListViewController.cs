using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public class CopyMoveToFolderListViewController : AbstractFoldersListViewController
    {
        readonly TaskCompletionSource<int?> tcs = new();
        public Task<int?> Result => tcs.Task;

        readonly List<IBusinessEntity> businessEntities;
        readonly Folder fromFolder;
        readonly bool delayedCopy = false;

        UIBarButtonItem cancelModeItem;

        public CopyMoveToFolderListViewController(ModuleType module, List<IBusinessEntity> businessEntities, Folder fromFolder = null, bool delayedCopy = false)
            : base(module, true, true, true)
        {
            this.businessEntities = businessEntities;
            this.fromFolder = fromFolder;
            this.delayedCopy = delayedCopy;
        }

        protected CopyMoveToFolderListViewController(Folder folder, List<IBusinessEntity> businessEntities, Folder fromFolder = null)
            : base(folder, true, true, true)
        {
            this.businessEntities = businessEntities;
            this.fromFolder = fromFolder;
        }

        protected override void InitializeNavigationBar()
        {
            string GetTitle()
            {
                switch (ParentFolder.Module)
                {
                    case ModuleType.Documents:
                        return Localization.GetString("documents");
                    case ModuleType.Contacts:
                        return Localization.GetString("contacts");
                    case ModuleType.Shortcodes:
                        return Localization.GetString("shortcodes");
                    case ModuleType.Calendar:
                        return Localization.GetString("contacts");
                    default:
                        return string.Empty;
                }
            };

            if (IsRootOfFoldersList)
                NavigationItem.Title = GetTitle();
            else
                NavigationItem.Title = ParentFolder.Name;

            if (IsRootOfFoldersList)
            {
                cancelModeItem = new UIBarButtonItem
                {
                    Title = Localization.GetString("cancel")
                };
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

        protected override async void FolderSelected(Folder folder, bool isFromFavorite)
        {
            if (folder.InternalType != FolderInternalType.FilterView
                && folder.InternalType != FolderInternalType.Static
                && folder.InternalType != FolderInternalType.Worktray)
            {
                await Dialogs.ShowConfirmAlertAsync(this,
                    Localization.GetString("failed"),
                    Localization.GetString("cannot_copy_or_move_to_dynamic_folder"));
                return;
            }

            if (delayedCopy == true)
            {
                tcs.SetResult(folder.Id);
                return;
            }

            bool done;
            if (fromFolder == null)
                done = await CopyBusinessEntityToFolder(folder);
            else
                done = await MoveBusinessEntityToFolder(folder);

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);
            if (done)
            {
                tcs.TrySetResult(folder.Id);
                DismissViewController(true, null);
            }

        }

        protected async override Task FolderExpand(Folder folder)
        {
            await base.FolderExpand(folder);

            var vc = new CopyMoveToFolderListViewController(folder, businessEntities, fromFolder);
            NavigationController.PushViewController(vc, true);
            await vc.Result;
            tcs.SetResult(vc.Result.Result);
        }

        protected override bool ShouldDisableFolder(Folder folder)
        {
            if (folder.Local)
                return true;

            return false;
        }

        void CancelModeItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
            tcs.TrySetResult(null);
        }

        async Task<bool> CopyBusinessEntityToFolder(Folder folder)
        {
            var confirmed = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("copy") }, View);
            if (confirmed < 0)
                return false;

            CommonConfig.Logger.Info($"Copying business entities to folder [businessEntities.Count={businessEntities?.Count}, " +
                $"businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("copying___"));

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(businessEntities, folder);
                dismissAction();
                return true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while copying business entities to folder [businessEntities.Count={businessEntities?.Count}, " +
                    $"businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
                tcs.SetResult(null);
                return false;
            }
            finally
            {
                DismissViewController(true, null);
            }
        }

        async Task<bool> MoveBusinessEntityToFolder(Folder folder)
        {
            var confirmed = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("move") }, View);
            if (confirmed < 0)
                return false;

            CommonConfig.Logger.Info($"Moving business entities to folder [businessEntities.Count={businessEntities?.Count}, " +
                $"businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("moving___"));

            try
            {
                await Managers.CommonActionsManager.MoveToFolder(businessEntities, fromFolder, folder);
                dismissAction();
                return true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entities to folder [businessEntities.Count={businessEntities?.Count}, " +
                    $"businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
                tcs.SetResult(null);
                return false;
            }
            finally
            {
                DismissViewController(true, null);
            }
        }
    }
}
