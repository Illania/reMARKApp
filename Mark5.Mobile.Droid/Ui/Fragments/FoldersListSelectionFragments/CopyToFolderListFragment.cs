//
// Project: Mark5.Mobile.Droid
// File: CopyToFolderListFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CopyToFolderListFragment : FoldersListFragment
    {
        public BusinessEntity BusinessEntity { get; set; }  //TODO Need to save this 

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new CopyToFolderListFragment
            {
                BusinessEntity = BusinessEntity,
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
            CommonConfig.Logger.Info($"Copying business entity to folder [businessEntity.Id={BusinessEntity.Id}, businessEntity.Type={BusinessEntity.ObjectType}, folder.Id={folder.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_folder, Resource.String.please_wait);

            var confirmed = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.confirm_copy_to_folder);
            if (!confirmed)
            {
                return;
            }

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(new List<IBusinessEntity> { BusinessEntity }, folder);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while copying business entity to folder [businessEntity.Id={BusinessEntity.Id}, businessEntity.Type={BusinessEntity.ObjectType}, folder.Id={folder.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Context, ex);
            }
            finally
            {
                dismissAction();
                Activity.Finish();
            }
        }

    }
}
