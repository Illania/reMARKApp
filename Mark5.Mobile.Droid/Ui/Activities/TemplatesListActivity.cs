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
    public class TemplatesListActivity : BaseAppCompatActivity
    {
        public const string TemplatePreviewResultKey = "TemplatePreviewResult_92796943-fe6c-44d1-a999-1d19286a97f6";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(TemplatesListActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(TemplatesListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.categories);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ft = SupportFragmentManager.BeginTransaction();
                var tlf = new TemplatesListFragment();
                ft.Replace(Resource.Id.fragment_container, tlf, tlf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(TemplatesListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(TemplatesListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}