//
// Project: Mark5.Mobile.Droid
// File: CopyToFolderListFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.HubMessages;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CopyMoveToFolderListFragment : FoldersListFragment
    {
        public enum ActionType
        {
            Copy,
            Move,
        };

        public List<IBusinessEntity> BusinessEntities { get; set; }
        public Folder FromFolder { get; set; }
        public ActionType Type { get; set; }
        override public bool LocalSectionEnabled { get; set; } = false;

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new CopyMoveToFolderListFragment
            {
                BusinessEntities = BusinessEntities,
                FromFolder = FromFolder,
                RemoteFolder = folder,
            };
        }

        protected override async void Adapter_ItemClicked(object sender, int position)
        {
            var toFolder = CurrentAdapter.GetItemAtPosition(position);
            if (Type == ActionType.Copy)
            {
                await CopyBusinessEntityToFolder(toFolder);
            }
            else
            {
                await MoveBusinessEntityToFolder(toFolder);
            }
        }

        async Task MoveBusinessEntityToFolder(Folder toFolder)
        {
            var title = GetString(Resource.String.confirm_move_to_folder);

            int resourceId = 0;
            switch (BusinessEntities.First().ObjectType)
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


            var content = Resources.GetQuantityString(resourceId, BusinessEntities.Count, toFolder.Name);
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, title, content);
            if (!confirmed)
            {
                return;
            }

            CommonConfig.Logger.Info($"Moving business entity to folder [businessEntities.Count={BusinessEntities?.Count}, businessEntity.Type={BusinessEntities.First().ObjectType}, toFolder.Id={toFolder?.Id}, fromFolder.Id={FromFolder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.moving_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.MoveToFolder(BusinessEntities, FromFolder, toFolder);
                PlatformConfig.MessengerHub.Publish(new EntityMovedFromFolderMessage(this, BusinessEntities.First().ObjectType, FromFolder.Id, BusinessEntities.Select(b => b.Id).ToList()));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entity to folder [businessEntities.Count={BusinessEntities?.Count}, businessEntity.Type={BusinessEntities?.First().ObjectType}, toFolder.Id={toFolder?.Id}, fromFolder.Id={FromFolder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
                Activity.Finish();
            }
        }

        async Task CopyBusinessEntityToFolder(Folder folder)
        {
            var title = Resources.GetString(Resource.String.confirm_copy_to_folder);

            int resourceId = 0;
            switch (BusinessEntities.First().ObjectType)
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

            var content = Resources.GetQuantityString(resourceId, BusinessEntities.Count, folder.Name);
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, title, content);

            if (!confirmed)
            {
                return;
            }

            CommonConfig.Logger.Info($"Copying business entities to folder [businessEntities.Count={BusinessEntities?.Count}, businessEntities.Type={BusinessEntities?.First().ObjectType}, folder.Id={folder?.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(BusinessEntities, folder);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while copying business entities to folder [businessEntities.Count={BusinessEntities?.Count}, businessEntities.Type={BusinessEntities?.First().ObjectType}, folder.Id={folder?.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
                Activity.Finish();
            }
        }

        #region Retained Fragment methods

        public override string GenerateTag()
        {
            return base.GenerateTag() + $" / {nameof(CopyMoveToFolderListFragment)} [businessEntities.Count={BusinessEntities?.Count}, businessEntity.Type={BusinessEntities?.First().ObjectType}, fromFolder.Id={FromFolder?.Id}]";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            var baseState = base.OnRetainInstanceState() as FolderListFragmentState;

            CommonConfig.Logger.Info($"Retaining state: [businessEntities.Count={BusinessEntities?.Count}, businessEntity.Type={BusinessEntities.First().ObjectType}, fromFolder.Id={FromFolder?.Id}]");

            return new MoveToFolderListFragmentState
            {
                Folder = baseState.Folder,
                SelectedItemPositions = baseState.SelectedItemPositions,
                BusinessEntities = BusinessEntities,
                FromFolder = FromFolder,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            base.OnRetainedInstanceStateRestored(restoredState as FolderListFragmentState);
            var flfs = restoredState as MoveToFolderListFragmentState;
            if (flfs != null)
            {
                BusinessEntities = flfs.BusinessEntities;
                FromFolder = flfs.FromFolder;
                CommonConfig.Logger.Info($"Restored state state: [businessEntities.Count={BusinessEntities?.Count}, businessEntity.Type={BusinessEntities?.First().ObjectType}, fromFolder.Id={FromFolder?.Id}]");
            }
        }

        protected class MoveToFolderListFragmentState : FolderListFragmentState
        {
            public List<IBusinessEntity> BusinessEntities { get; set; }
            public Folder FromFolder { get; set; }
        }

        #endregion

    }
}
