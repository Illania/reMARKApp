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

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CopyToFolderListFragment : FoldersListFragment
    {
        public List<IBusinessEntity> BusinessEntities { get; set; }
        override public bool LocalSectionAvailable { get; set; } = false;

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new CopyToFolderListFragment
            {
                BusinessEntities = BusinessEntities,
                Folder = folder,
            };
        }

        protected override async void Adapter_ItemClicked(object sender, int position)
        {
            var folder = CurrentAdapter.GetItemAtPosition(position);
            await CopyBusinessEntityToFolder(folder);
        }

        async Task CopyBusinessEntityToFolder(Folder folder)
        {
            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.confirm_copy_to_folder);
            if (!confirmed)
            {
                return;
            }

            CommonConfig.Logger.Info($"Copying business entities to folder [businessEntities.Count={BusinessEntities.Count}, businessEntities.Type={BusinessEntities.First().ObjectType}, folder.Id={folder.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(BusinessEntities, folder);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while copying business entities to folder [businessEntities.Count={BusinessEntities.Count}, businessEntities.Type={BusinessEntities.First().ObjectType}, folder.Id={folder.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Context, ex);
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
            return base.GenerateTag() + $" / {nameof(CopyToFolderListFragment)} [businessEntities.Count={BusinessEntities.Count}, businessEntities.Type={BusinessEntities.First().ObjectType}]";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            var baseState = base.OnRetainInstanceState() as FolderListFragmentState;

            CommonConfig.Logger.Info($"Retaining state: [businessEntities.Count={BusinessEntities.Count}, businessEntities.Type={BusinessEntities.First().ObjectType}]");

            return new CopyToFolderListFragmentState
            {
                Folder = baseState.Folder,
                SelectedItemPositions = baseState.SelectedItemPositions,
                BusinessEntity = BusinessEntities,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            base.OnRetainedInstanceStateRestored(restoredState as FolderListFragmentState);
            var flfs = restoredState as CopyToFolderListFragmentState;
            if (flfs != null)
            {
                BusinessEntities = flfs.BusinessEntity;
                CommonConfig.Logger.Info($"Restored state state: [businessEntities.Count={BusinessEntities.Count}, businessEntities.Type={BusinessEntities.First().ObjectType}]");
            }
        }

        protected class CopyToFolderListFragmentState : FolderListFragmentState
        {
            public List<IBusinessEntity> BusinessEntity { get; set; }
        }

        #endregion
    }
}
