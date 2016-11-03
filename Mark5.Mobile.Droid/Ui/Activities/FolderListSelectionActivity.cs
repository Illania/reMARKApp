//
// Project: Mark5.Mobile.Droid
// File: FolderListSelectionActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity]
    public class FolderListSelectionActivity : BaseAppCompatActivity
    {
        public enum ModeType
        {
            CopyToFolderMode = 1,
            MoveToFolderMode = 2,
            PickerMode = 3,
        }

        public const string ModuleIntentKey = "ModuleIntent_79a3dba4-bdad-4b11-be42-af6acdf31b4e";
        public const string ModeIntentKey = "ModeIntent_418bec8d-f44d-41b4-bff0-e286dea3d705";
        public const string BusinessEntitiesIntentKey = "BusinessEntitiesIntent_d6047bae-dc5e-4c3e-a302-e33931531baa";
        public const string FromFolderIntentKey = "FromFolderIntent_3a68d401-f581-4094-b526-4478cc43d3f4";

        public const string FoldersResultKey = "FoldersResult_32e7327a-f02e-4628-850a-6d86e2109b3e";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentsListActivity)}...");

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var listMode = (ModeType)Intent.Extras.GetInt(ModeIntentKey);
                var moduleType = SerializationUtils.Deserialize<ModuleType>(Intent.Extras.GetString(ModuleIntentKey));
                var be = Intent.HasExtra(BusinessEntitiesIntentKey) ? SerializationUtils.Deserialize<List<IBusinessEntity>>(Intent.Extras.GetString(BusinessEntitiesIntentKey)) : null;
                var fromFolder = Intent.HasExtra(FromFolderIntentKey) ? SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FromFolderIntentKey)) : null;

                var ft = SupportFragmentManager.BeginTransaction();

                if (listMode == ModeType.CopyToFolderMode)
                {
                    SupportActionBar.SetTitle(Resource.String.select_folder);
                    var flf = new CopyToFolderListFragment
                    {
                        Folder = Folder.RootPerModule(moduleType),
                        BusinessEntities = be,
                    };
                    ft.Replace(Resource.Id.fragment_container, flf, flf.GenerateTag());
                }
                else if (listMode == ModeType.MoveToFolderMode)
                {
                    SupportActionBar.SetTitle(Resource.String.select_folder);
                    var flf = new MoveToFolderListFragment
                    {
                        Folder = Folder.RootPerModule(moduleType),
                        BusinessEntities = be,
                        FromFolder = fromFolder,
                    };
                    ft.Replace(Resource.Id.fragment_container, flf, flf.GenerateTag());
                }
                else if (listMode == ModeType.PickerMode)
                {
                    SupportActionBar.SetTitle(Resource.String.select_folders);
                    var flf = new PickerFolderListFragment
                    {
                        Folder = Folder.RootPerModule(moduleType),
                    };
                    ft.Replace(Resource.Id.fragment_container, flf, flf.GenerateTag());
                }

                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(FolderListSelectionActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(FolderListSelectionActivity)}");
            }

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                OnBackPressed();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }

}
