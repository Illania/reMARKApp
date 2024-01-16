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
        private readonly TaskCompletionSource<int?> _tcs = new();
        public Task<int?> Result => _tcs.Task;

        private readonly List<IBusinessEntity> _businessEntities;
        private readonly Folder _fromFolder;
        private readonly bool _delayedCopy;
        private UIBarButtonItem _cancelModeItem;

        public CopyMoveToFolderListViewController(ModuleType module, List<IBusinessEntity> businessEntities, Folder fromFolder = null, bool delayedCopy = false)
            : base(module, true, true, true)
        {
            this._businessEntities = businessEntities;
            this._fromFolder = fromFolder;
            this._delayedCopy = delayedCopy;
        }

        protected CopyMoveToFolderListViewController(Folder folder, List<IBusinessEntity> businessEntities, Folder fromFolder = null)
            : base(folder, true, true, true)
        {
            this._businessEntities = businessEntities;
            this._fromFolder = fromFolder;
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
            }

            NavigationItem.Title = IsRootOfFoldersList ? GetTitle() : ParentFolder.Name;

            if (!IsRootOfFoldersList)
                return;

            _cancelModeItem = new UIBarButtonItem
            {
                Title = Localization.GetString("cancel")
            };
            NavigationItem.SetLeftBarButtonItem(_cancelModeItem, false);
        }

        protected override void InitializeHandlers()
        {
            base.InitializeHandlers();

            if (_cancelModeItem != null)
                _cancelModeItem.Clicked += CancelModeItem_Clicked;
        }

        protected override void DeinitializeHandlers()
        {
            base.DeinitializeHandlers();

            if (_cancelModeItem != null)
                _cancelModeItem.Clicked -= CancelModeItem_Clicked;
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

            if (_delayedCopy)
            {
                _tcs.SetResult(folder.Id);
                return;
            }

            bool done;
            if (_fromFolder == null)
                done = await CopyBusinessEntityToFolder(folder);
            else
                done = await MoveBusinessEntityToFolder(folder);

            if (TableView.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

            if (!done)
                return;

            _tcs.TrySetResult(folder.Id);
            DismissViewController(true, null);
        }

        protected override async Task FolderExpand(Folder folder)
        {
            await base.FolderExpand(folder);

            var vc = new CopyMoveToFolderListViewController(folder, _businessEntities, _fromFolder);
            NavigationController?.PushViewController(vc, true);
            await vc.Result;
            _tcs.SetResult(vc.Result.Result);
        }

        protected override bool ShouldDisableFolder(Folder folder) => folder.Local;

        void CancelModeItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
            _tcs.TrySetResult(null);
        }

        async Task<bool> CopyBusinessEntityToFolder(Folder folder)
        {
            var confirmed = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("copy") }, View);
            if (confirmed < 0)
                return false;

            CommonConfig.Logger.Info($"Copying business entities to folder [businessEntities.Count={_businessEntities?.Count}, " +
                $"businessEntities.Type={_businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("copying___"));

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(_businessEntities, folder);
                dismissAction();
                return true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while copying business entities to folder [businessEntities.Count={_businessEntities?.Count}, " +
                    $"businessEntities.Type={_businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
                _tcs.SetResult(null);
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

            CommonConfig.Logger.Info($"Moving business entities to folder [businessEntities.Count={_businessEntities?.Count}, " +
                $"businessEntities.Type={_businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("moving___"));

            try
            {
                await Managers.CommonActionsManager.MoveToFolder(_businessEntities, _fromFolder, folder);
                dismissAction();
                return true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entities to folder [businessEntities.Count={_businessEntities?.Count}, " +
                    $"businessEntities.Type={_businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
                _tcs.SetResult(null);
                return false;
            }
            finally
            {
                DismissViewController(true, null);
            }
        }
    }
}
