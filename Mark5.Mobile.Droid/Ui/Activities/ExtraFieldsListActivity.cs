using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
     [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize )]
    public class ExtraFieldsListActivity : BaseAppCompatActivity
    {

        ExtraFieldsListFragment cf;

        public static Intent CreateIntent(Context context)
        {
            var intent = new Intent(context, typeof(ExtraFieldsListActivity));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ExtraFieldsListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.extra_fields);
            SetContentView(Resource.Layout.base_layout);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                string tag;
              
                var ft = SupportFragmentManager.BeginTransaction();
                (cf, tag) = ExtraFieldsListFragment.NewInstance();
                ft.Replace(Resource.Id.fragment_container, cf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ExtraFieldsListActivity)}");
            }
            else
            {
                cf = (ExtraFieldsListFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(ExtraFieldsListActivity)}");
            }
        }

        public override async void OnBackPressed()
        {
            if (cf != null)
            {
                await Managers.DocumentsManager.UpdateExtraFieldsAsync(cf.ExtraFields);
                SetResult(Result.Ok); 
            }
            base.OnBackPressed();
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}