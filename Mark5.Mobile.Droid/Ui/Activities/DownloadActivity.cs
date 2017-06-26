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
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class DownloadActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "FolderIntent_07bbbc0a-5453-4557-881f-2599cfb99a9e";

        DownloadFragment df;

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
                df = new DownloadFragment
                {
                    Folder = folder
                };
                ft.Replace(Resource.Id.fragment_container, df, df.GenerateTag());
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
