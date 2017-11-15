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
        protected override bool LoadRemoteFromCache => true;

        readonly List<IBusinessEntity> businessEntities;
        readonly Folder fromFolder;

        UIBarButtonItem cancelModeItem;

        public CopyMoveToFolderListViewController(ModuleType module, List<IBusinessEntity> businessEntities, Folder fromFolder = null)
            : base(module, true, true, true)
        {
            this.businessEntities = businessEntities;
            this.fromFolder = fromFolder;
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
            base.FolderSelected(folder, isFromFavorite);

            if (fromFolder == null)
                await CopyBusinessEntityToFolder(folder);
            else
                await MoveBusinessEntityToFolder(folder);

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);
        }

        protected override void FolderExpand(Folder folder)
        {
            base.FolderExpand(folder);

            var vc = new CopyMoveToFolderListViewController(folder, businessEntities, fromFolder);
            NavigationController.PushViewController(vc, true);
        }

        protected override bool ShouldDisableFolder(Folder folder)
        {
            if (folder.Local)
                return true;

            if (folder.InternalType != FolderInternalType.FilterView && folder.InternalType != FolderInternalType.Static && folder.InternalType != FolderInternalType.Worktray)
                return true;

            return false;
        }

        void CancelModeItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        async Task CopyBusinessEntityToFolder(Folder folder)
        {
            var confirmed = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("copy") }, View);
            if (confirmed < 0)
                return;

            CommonConfig.Logger.Info($"Copying business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("copying___"));

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(businessEntities, folder);
                dismissAction();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while copying business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                DismissViewController(true, null);
            }
        }

        async Task MoveBusinessEntityToFolder(Folder folder)
        {
            var confirmed = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("move") }, View);
            if (confirmed < 0)
                return;

            CommonConfig.Logger.Info($"Moving business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("moving___"));

            try
            {
                await Managers.CommonActionsManager.MoveToFolder(businessEntities, fromFolder, folder);
                dismissAction();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                DismissViewController(true, null);
            }
        }
    }
}