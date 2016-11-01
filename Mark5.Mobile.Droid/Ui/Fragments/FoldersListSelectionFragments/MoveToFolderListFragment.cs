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
using Mark5.Mobile.Droid.Ui.Common.BusMesseges;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class MoveToFolderListFragment : FoldersListFragment
    {
        public List<IBusinessEntity> BusinessEntities { get; set; }
        public Folder FromFolder { get; set; }

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new MoveToFolderListFragment
            {
                BusinessEntities = BusinessEntities,
                FromFolder = FromFolder,
                Folder = folder,
            };
        }

        protected override async void Adapter_ItemClicked(object sender, int position)
        {
            var toFolder = CurrentAdapter.GetItemAtPosition(position);
            await MoveBusinessEntityToFolder(toFolder);
        }

        async Task MoveBusinessEntityToFolder(Folder toFolder)
        {
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.confirm_move_to_folder);
            if (!confirmed)
            {
                return;
            }

            CommonConfig.Logger.Info($"Moving business entity to folder [businessEntities.Count={BusinessEntities.Count}, businessEntity.Type={BusinessEntities.First().ObjectType}, toFolder.Id={toFolder.Id}, fromFolder.Id={FromFolder.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.moving_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.MoveToFolder(BusinessEntities, FromFolder, toFolder);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entity to folder [businessEntities.Count={BusinessEntities.Count}, businessEntity.Type={BusinessEntities.First().ObjectType}, toFolder.Id={toFolder.Id}, fromFolder.Id={FromFolder.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Context, ex);
            }
            finally
            {
                dismissAction();
                PlatformConfig.MessengerHub.Publish(new EntityMovedFromFolderMessage(this, BusinessEntities.First().ObjectType, FromFolder.Id, BusinessEntities.Select(b => b.Id).ToList()));
                Activity.Finish();
            }
        }

        #region Retained Fragment methods

        public override string GenerateTag()
        {
            return base.GenerateTag() + $" / {nameof(MoveToFolderListFragment)} [businessEntities.Count={BusinessEntities.Count}, businessEntity.Type={BusinessEntities.First().ObjectType}, fromFolder.Id={FromFolder.Id}]";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            var baseState = base.OnRetainInstanceState() as FolderListFragmentState;

            CommonConfig.Logger.Info($"Retaining state: [businessEntities.Count={BusinessEntities.Count}, businessEntity.Type={BusinessEntities.First().ObjectType}, fromFolder.Id={FromFolder.Id}]");

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
                CommonConfig.Logger.Info($"Restored state state: [businessEntities.Count={BusinessEntities.Count}, businessEntity.Type={BusinessEntities.First().ObjectType}, fromFolder.Id={FromFolder.Id}]");
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
