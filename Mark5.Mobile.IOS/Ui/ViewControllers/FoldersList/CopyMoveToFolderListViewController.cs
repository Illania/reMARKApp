using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Model.HubMessages;
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

        public CopyMoveToFolderListViewController(List<IBusinessEntity> businessEntities, Folder fromFolder = null)
            : base(businessEntities.FirstOrDefault().ModuleType, true, true, true)
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

        protected override void InitializeNavigationBarTitle()
        {
            Func<string> getTitle = () =>
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

            UIView.AnimationsEnabled = false;
            if (IsRootOfFoldersList)
            {
                NavigationItem.Title = fromFolder == null ? Localization.GetString("copy_to_folder") : Localization.GetString("move_to_folder");
                NavigationItem.Prompt = getTitle();
            }
            else
            {
                NavigationItem.Title = ParentFolder.Name;
                NavigationItem.Prompt = getTitle();
            }
            UIView.AnimationsEnabled = true;
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

        protected override void FolderSelected(Folder folder)
        {
            base.FolderSelected(folder);

            if (fromFolder == null)
                CopyBusinessEntityToFolder(folder);
            else
                MoveBusinessEntityToFolder(folder);
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

        void CancelModeItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void CopyBusinessEntityToFolder(Folder folder)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            string key;
            switch (businessEntities.First().ObjectType)
            {
                case ObjectType.Document:
                    key = "confirm_copy_to_folder_documents";
                    break;
                case ObjectType.Contact:
                    key = "confirm_copy_to_folder_contacts";
                    break;
                case ObjectType.Shortcode:
                    key = "confirm_copy_to_folder_shortcodes";
                    break;
                default:
                    throw new ArgumentException("Object type not supported!");
            }

            var confirmed = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("confirm_copy_to_folder"), Localization.GetString(key));
            if (!confirmed)
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
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                DismissViewController(true, null);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void MoveBusinessEntityToFolder(Folder folder)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            string key;
            switch (businessEntities.First().ObjectType)
            {
                case ObjectType.Document:
                    key = "confirm_move_to_folder_documents";
                    break;
                case ObjectType.Contact:
                    key = "confirm_move_to_folder_contacts";
                    break;
                case ObjectType.Shortcode:
                    key = "confirm_move_to_folder_shortcodes";
                    break;
                default:
                    throw new ArgumentException("Object type not supported!");
            }

            var confirmed = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("confirm_move_to_folder"), Localization.GetString(key));
            if (!confirmed)
                return;

            CommonConfig.Logger.Info($"Moving business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("moving___"));

            try
            {
                await Managers.CommonActionsManager.MoveToFolder(businessEntities, fromFolder, folder);
                PlatformConfig.MessengerHub.Publish(new EntityMovedFromFolderMessage(this, businessEntities.First().ObjectType, fromFolder.Id, businessEntities.Select(b => b.Id).ToList()));
                dismissAction();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                DismissViewController(true, null);
            }
        }
    }
}