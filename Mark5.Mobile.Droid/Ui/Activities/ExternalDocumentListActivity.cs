using Android.App;
using Android.Content;
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
     [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize )]
    public class ExternalDocumentsListActivity : BaseAppCompatActivity
    {
        public const string AttachmentResultKey = "bd52c99b-6fd5-400e-a63f-24e02e44ca11";
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        const string dlfFragmentTagKey = "DocumentsListFragmentTagKey";
        const string dtulfFragmentTagKey = "DocumentsToUploadListFragmentTagKey";

        Toolbar toolbar;

        ExternalDocumentsListFragment edlf;

        string edlfFragmentTag;

        public static Intent CreateIntent(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(ExternalDocumentsListActivity));
            intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ExternalDocumentsListActivity)}...");

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

                (edlf, edlfFragmentTag) = ExternalDocumentsListFragment.NewInstance(folder);
                ft.Replace(Resource.Id.fragment_container, edlf, edlfFragmentTag);

                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ExternalDocumentsListActivity)}");
            }
            else
            {
                edlfFragmentTag = savedInstanceState.GetString(dlfFragmentTagKey);
                if (!string.IsNullOrEmpty(edlfFragmentTag))
                {
                    edlf = SupportFragmentManager.FindFragmentByTag(edlfFragmentTag) as ExternalDocumentsListFragment;
                    CommonConfig.Logger.Info($"Reassigned {nameof(ExternalDocumentsListFragment)}");
                }

                CommonConfig.Logger.Info($"Restored {nameof(ExternalDocumentsListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(dlfFragmentTagKey, edlfFragmentTag);

            base.OnSaveInstanceState(outState);
        }
    }
}