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
    public class MoveToFolderListFragment : FoldersListFragment
    {
        public BusinessEntity BusinessEntity { get; set; }  //TODO Need to save this 
        public Folder FromFolder { get; set; }  //TODO Need to save this 

        protected override async void Adapter_ItemClicked(object sender, int position)
        {
            var toFolder = CurrentAdapter.GetItemAtPosition(position);
            await MoveBusinessEntityToFolder(toFolder);
        }

        async Task MoveBusinessEntityToFolder(Folder toFolder)
        {
            CommonConfig.Logger.Info($"Moving business entity to folder [businessEntity.Id={BusinessEntity.Id}, businessEntity.Type={BusinessEntity.ObjectType}, toFolder.Id={toFolder.Id}, fromFolder.Id={FromFolder.Id}]");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.CopyToFolder(new List<IBusinessEntity> { BusinessEntity }, toFolder);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while moving business entity to folder [businessEntity.Id={BusinessEntity.Id}, businessEntity.Type={BusinessEntity.ObjectType}, toFolder.Id={toFolder.Id}, fromFolder.Id={FromFolder.Id}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Context, ex);
            }
            finally
            {
                dismissAction();
            }
        }

    }
}
