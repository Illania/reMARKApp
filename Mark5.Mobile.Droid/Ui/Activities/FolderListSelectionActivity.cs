//
// Project: Mark5.Mobile.Droid
// File: FolderListSelectionActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

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
            SimpleMode = 1,
            CopyToFolderMode = 2,
            MoveToFolderMode = 3,
            PickerMode = 4,
        }

        public const string ModuleIntentKey = "ModuleIntent_79a3dba4-bdad-4b11-be42-af6acdf31b4e";
        public const string ModeIntentKey = "Mode_418bec8d-f44d-41b4-bff0-e286dea3d705";
        public const string BusinessEntityIntentKey = "BusinessEntity_d6047bae-dc5e-4c3e-a302-e33931531baa";
        public const string FromFolderIntentKey = "FromFolderIntent_3a68d401-f581-4094-b526-4478cc43d3f4";

        public const string FoldersResultKey = "FoldersResult_32e7327a-f02e-4628-850a-6d86e2109b3e";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentsListActivity)}...");

            //TODO need to set the correct title according to action?
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var listMode = (ModeType)Intent.Extras.GetInt(ModeIntentKey);
                var moduleType = SerializationUtils.Deserialize<ModuleType>(Intent.Extras.GetString(ModuleIntentKey));
                var be = Intent.HasExtra(BusinessEntityIntentKey) ? SerializationUtils.Deserialize<BusinessEntity>(Intent.Extras.GetString(BusinessEntityIntentKey)) : null;
                var fromFolder = Intent.HasExtra(FromFolderIntentKey) ? SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FromFolderIntentKey)) : null;

                var ft = SupportFragmentManager.BeginTransaction();

                if (listMode == ModeType.CopyToFolderMode)
                {
                    var flf = new CopyToFolderListFragment
                    {
                        Folder = Folder.RootPerModule(moduleType),
                        BusinessEntity = be,
                    };
                    ft.Replace(Resource.Id.fragment_container, flf, flf.GenerateTag());
                }
                else if (listMode == ModeType.MoveToFolderMode)
                {
                    var flf = new MoveToFolderListFragment
                    {
                        Folder = Folder.RootPerModule(moduleType),
                        BusinessEntity = be,
                        FromFolder = fromFolder,
                    };
                    ft.Replace(Resource.Id.fragment_container, flf, flf.GenerateTag());
                }
                else if (listMode == ModeType.PickerMode)
                {
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
