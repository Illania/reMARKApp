
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class RecentAddressesListActivity : BaseAppCompatActivity
    {
        public const string RecipientResultKey = "CategoriesResult_0b8c55ac-2dbe-441e-af92-daa330d040fe";

        Toolbar toolbar;
        RecentAddressesListFragment ralf;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(RecentAddressesListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.recent_addresses);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ft = SupportFragmentManager.BeginTransaction();
                ralf = new RecentAddressesListFragment();
                ft.Replace(Resource.Id.fragment_container, ralf, ralf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(RecentAddressesListActivity)}");
            }
            else
            {
                ralf = (RecentAddressesListFragment) SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(RecentAddressesListActivity)}");
            }
        }

        public override void OnBackPressed()
        {
            //if (clf != null)
            //{
            //    var intent = new Intent();
            //    intent.PutExtra(RecipientResultKey, SerializationUtils.Serialize(clf.Categories));
            //    SetResult(Result.Ok, intent);
            //}

            base.OnBackPressed();
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}

