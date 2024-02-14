using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Fragments;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace reMark.Mobile.Droid.Ui.Activities
{
     [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize )]
    public class DownloadActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "FolderIntent_07bbbc0a-5453-4557-881f-2599cfb99a9e";

        DownloadFragment df;
        string dfFragmentTag;

        public static Intent CreateIntent(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(DownloadActivity));
            intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DownloadActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.document);
            SetContentView(Resource.Layout.base_layout);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                (df, dfFragmentTag) = DownloadFragment.NewInstance(folder);
                ft.Replace(Resource.Id.fragment_container, df, dfFragmentTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(DownloadActivity)}");
            }
            else
            {
                df = (DownloadFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(DownloadActivity)}");
            }
        }

        public override void OnBackPressed()
        {
            if (!df.OnBackPressed())
                return;

            if (df != null)
                SetResult(Result.Ok);

            base.OnBackPressed();
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}