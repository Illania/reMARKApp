using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class CopyMoveToFolderListActivity : BaseAppCompatActivity
    {
        public enum ModeType
        {
            Copy = 1,
            Move = 2,
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

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var listMode = (ModeType) Intent.Extras.GetInt(ModeIntentKey);
                var moduleType = SerializationUtils.Deserialize<ModuleType>(Intent.Extras.GetString(ModuleIntentKey));
                var be = Intent.HasExtra(BusinessEntitiesIntentKey) ? SerializationUtils.Deserialize<List<IBusinessEntity>>(Intent.Extras.GetString(BusinessEntitiesIntentKey)) : null;
                var fromFolder = Intent.HasExtra(FromFolderIntentKey) ? SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FromFolderIntentKey)) : null;

                var ft = SupportFragmentManager.BeginTransaction();

                switch (listMode)
                {
                    case ModeType.Copy:
                        SupportActionBar.SetTitle(Resource.String.select_folder);
                        var cmflf = new CopyMoveToFolderListFragment
                        {
                            RemoteFolder = Folder.RootForModule(moduleType),
                            BusinessEntities = be,
                            Type = CopyMoveToFolderListFragment.ActionType.Copy
                        };
                        ft.Replace(Resource.Id.fragment_container, cmflf, cmflf.GenerateTag());
                        break;
                    case ModeType.Move:
                        SupportActionBar.SetTitle(Resource.String.select_folder);
                        var cmflf2 = new CopyMoveToFolderListFragment
                        {
                            RemoteFolder = Folder.RootForModule(moduleType),
                            BusinessEntities = be,
                            FromFolder = fromFolder,
                            Type = CopyMoveToFolderListFragment.ActionType.Move
                        };
                        ft.Replace(Resource.Id.fragment_container, cmflf2, cmflf2.GenerateTag());
                        break;
                }

                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CopyMoveToFolderListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(CopyMoveToFolderListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}