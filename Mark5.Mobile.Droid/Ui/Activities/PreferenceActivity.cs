using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class PreferenceActivity : BaseAppCompatActivity
    {
        Toolbar toolbar;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(PreferenceActivity)); 
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PreferenceFragment)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.settings);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ft = SupportFragmentManager.BeginTransaction();
                var (paf, tag) = PreferenceFragment.NewInstance();
                ft.Replace(Resource.Id.fragment_container, paf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PreferenceFragment)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PreferenceFragment)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}