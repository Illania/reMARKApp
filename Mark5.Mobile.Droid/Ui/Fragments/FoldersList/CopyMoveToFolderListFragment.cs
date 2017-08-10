using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Model.HubMessages;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CopyMoveToFolderListFragment : FoldersListFragment
    {
        protected override bool LoadRemoteFromCache => true;

        List<IBusinessEntity> businessEntities;
        Folder fromFolder;
        ActionType type;

        public CopyMoveToFolderListFragment() { }

        public CopyMoveToFolderListFragment(Folder remoteFolder, List<IBusinessEntity> businessEntities, Folder fromFolder = null, ActionType? actionType = null)
        {
            if (remoteFolder != null)
                RemoteFolder = remoteFolder;
            
            if (businessEntities != null)
                this.businessEntities = businessEntities;
            
            if (fromFolder != null)
                this.fromFolder = fromFolder;
            
            if (actionType != null)
                type = (ActionType)actionType;
        }

        protected override void SetSections()
        {
            CommonConfig.Logger.Info("Setting sections according to the folder");

            if (RemoteFolder.Root)
                AvailableSections = new List<Section>
                {
                    Section.Favourites,
                    Section.Remote
                };
            else
                AvailableSections = new List<Section>
                {
                    Section.Remote
                };

            Adapter.SetSections(AvailableSections);
        }

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new CopyMoveToFolderListFragment
            {
                businessEntities = businessEntities,
                fromFolder = fromFolder,
                RemoteFolder = folder,
                type = type
            };
        }

        protected override async void Adapter_ItemClicked(object sender, int position)
        {
            var toFolder = CurrentAdapter.GetItemAtPosition(position);
            if (type == ActionType.Copy)
                await CopyBusinessEntityToFolder(toFolder);
            else
                await MoveBusinessEntityToFolder(toFolder);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
        }

        async Task MoveBusinessEntityToFolder(Folder toFolder)
        {
            var title = GetString(Resource.String.confirm_move_to_folder);

            var resourceId = 0;
            switch (businessEntities.First().ObjectType)
            {
                case ObjectType.Document:
                    resourceId = Resource.Plurals.confirm_move_to_folder_documents;
                    break;
                case ObjectType.Contact:
                    resourceId = Resource.Plurals.confirm_move_to_folder_contacts;
                    break;
                case ObjectType.Shortcode:
                    resourceId = Resource.Plurals.confirm_move_to_folder_shortcodes;
                    break;
                default:
                    throw new ArgumentException("Object type not supported!");
            }


            var content = Resources.GetQuantityString(resourceId, businessEntities.Count, toFolder.Name);
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, title, content);
            if (!confirmed)
                return;

            CommonConfig.Logger.Info($"Moving business entity to folder [businessEntities.Count={businessEntities?.Count}, businessEntity.Type={businessEntities.First().ObjectType}, toFolder.Id={toFolder?.Id}, fromFolder.Id={fromFolder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.moving_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.MoveToFolder(businessEntities, fromFolder, toFolder);
                CommonConfig.MessengerHub.Publish(new EntityMovedFromFolderMessage(this, businessEntities.First().ObjectType, fromFolder.Id, businessEntities.Select(b => b.Id).ToList()));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entity to folder [businessEntities.Count={businessEntities?.Count}, businessEntity.Type={businessEntities?.First().ObjectType}, toFolder.Id={toFolder?.Id}, fromFolder.Id={fromFolder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
                Activity?.Finish();
            }
        }

        async Task CopyBusinessEntityToFolder(Folder folder)
        {
            var title = Resources.GetString(Resource.String.confirm_copy_to_folder);

            var resourceId = 0;
            switch (businessEntities.First().ObjectType)
            {
                case ObjectType.Document:
                    resourceId = Resource.Plurals.confirm_copy_to_folder_documents;
                    break;
                case ObjectType.Contact:
                    resourceId = Resource.Plurals.confirm_copy_to_folder_contacts;
                    break;
                case ObjectType.Shortcode:
                    resourceId = Resource.Plurals.confirm_copy_to_folder_shortcodes;
                    break;
                default:
                    throw new ArgumentException("Object type not supported!");
            }

            var content = Resources.GetQuantityString(resourceId, businessEntities.Count, folder.Name);
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, title, content);

            if (!confirmed)
                return;

            CommonConfig.Logger.Info($"Copying business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(businessEntities, folder);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while copying business entities to folder [businessEntities.Count={businessEntities?.Count}, businessEntities.Type={businessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
                Activity?.Finish();
            }
        }

        #region Retained Fragment methods

        public override string GenerateTag()
        {
            return base.GenerateTag() + $" / {nameof(CopyMoveToFolderListFragment)} [businessEntities.Count={businessEntities.Count}, businessEntity.Type={businessEntities.First().ObjectType}, fromFolder.Id={fromFolder?.Id ?? -1}]";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            var baseState = base.OnRetainInstanceState() as FolderListFragmentState;

            CommonConfig.Logger.Info($"Retaining state: [businessEntities.Count={businessEntities?.Count}, businessEntity.Type={businessEntities.First().ObjectType}, fromFolder.Id={fromFolder?.Id}]");

            return new MoveToFolderListFragmentState
            {
                Folder = baseState.Folder,
                SelectedItemPositions = baseState.SelectedItemPositions,
                BusinessEntities = businessEntities,
                FromFolder = fromFolder,
                Type = type
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            base.OnRetainedInstanceStateRestored(restoredState as FolderListFragmentState);

            if (restoredState is MoveToFolderListFragmentState flfs)
            {
                businessEntities = flfs.BusinessEntities;
                fromFolder = flfs.FromFolder;
                type = flfs.Type;
                CommonConfig.Logger.Info($"Restored state state: [businessEntities.Count={businessEntities?.Count}, businessEntity.Type={businessEntities?.First().ObjectType}, fromFolder.Id={fromFolder?.Id}]");
            }
        }

        protected class MoveToFolderListFragmentState : FolderListFragmentState
        {
            public List<IBusinessEntity> BusinessEntities { get; set; }
            public Folder FromFolder { get; set; }
            public ActionType Type { get; set; }
        }

        #endregion

        public enum ActionType
        {
            Copy,
            Move,
        };
    }
}