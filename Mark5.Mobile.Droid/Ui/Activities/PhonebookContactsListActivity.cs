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
    public class PhonebookContactsListActivity : BaseAppCompatActivity
    {
        public const string RecipientResultKey = "RecipientResult_30f359c2-8e26-4149-979a-80fd76d7d118";

        Toolbar toolbar;

        PhonebookContactsListFragment plf;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(PhonebookContactsListActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PhonebookContactsListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.recent_addresses);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ft = SupportFragmentManager.BeginTransaction();
                plf = new PhonebookContactsListFragment();
                ft.Replace(Resource.Id.fragment_container, plf, plf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PhonebookContactsListActivity)}");
            }
            else
            {
                plf = (PhonebookContactsListFragment) SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(PhonebookContactsListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}