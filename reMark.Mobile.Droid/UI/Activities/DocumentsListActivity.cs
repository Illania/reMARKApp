using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Fragments;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace reMark.Mobile.Droid.Ui.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize )]
    public class DocumentsListActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        const string dlfFragmentTagKey = "DocumentsListFragmentTagKey";
        const string dtulfFragmentTagKey = "DocumentsToUploadListFragmentTagKey";

        Toolbar toolbar;

        DocumentsUnreadDocumentsListFragment dlf;
        DocumentsToUploadListFragment odlf;

        string dlfFragmentTag;
        string dtuFragmentTag;

        public static Intent CreateIntent(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(DocumentsListActivity));
            intent.PutExtra(FolderIntentKey,Serializer.Serialize(folder));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentsListActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetTitle(Resource.String.documents);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                if (folder.Local && folder.Id == Folder.LocalRootForModule(ModuleType.Documents).SubFolders[0].Id)
                {
                    (odlf, dtuFragmentTag) = DocumentsToUploadListFragment.NewInstance();
                    ft.Replace(Resource.Id.fragment_container, odlf, dtuFragmentTag);
                }
                else
                {
                    (dlf, dlfFragmentTag) = DocumentsUnreadDocumentsListFragment.NewInstance(folder);
                    ft.Replace(Resource.Id.fragment_container, dlf, dlfFragmentTag);
                }
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(DocumentsListActivity)}");
            }
            else
            {
                dlfFragmentTag = savedInstanceState.GetString(dlfFragmentTagKey);
                if (!string.IsNullOrEmpty(dlfFragmentTag))
                {
                    dlf = SupportFragmentManager.FindFragmentByTag(dlfFragmentTag) as DocumentsUnreadDocumentsListFragment;
                    CommonConfig.Logger.Info($"Reassigned {nameof(DocumentsListFragment)}");
                }

                dtuFragmentTag = savedInstanceState.GetString(dtulfFragmentTagKey);
                if (!string.IsNullOrEmpty(dtuFragmentTag))
                {
                    odlf = SupportFragmentManager.FindFragmentByTag(dtuFragmentTag) as DocumentsToUploadListFragment;
                    CommonConfig.Logger.Info($"Reassigned {nameof(DocumentsToUploadListFragment)}");
                }

                CommonConfig.Logger.Info($"Restored {nameof(DocumentsListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(dlfFragmentTagKey, dlfFragmentTag);
            outState.PutString(dtulfFragmentTagKey, dtuFragmentTag);

            base.OnSaveInstanceState(outState);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}