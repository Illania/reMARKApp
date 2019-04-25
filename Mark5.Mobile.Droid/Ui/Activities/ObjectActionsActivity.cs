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
    public class ObjectActionsActivity : BaseAppCompatActivity
    {
        public const string BusinessEntityIntentKey = "BusinessEntity_ef8f3886-1478-4b4c-8bdb-7a6188035674";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, IBusinessEntity businessEntity)
        {
            var intent = new Intent(context, typeof(ObjectActionsActivity));
            intent.PutExtra(BusinessEntityIntentKey, Serializer.Serialize(businessEntity));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ObjectActionsActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.history);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var be = Serializer.Deserialize<IBusinessEntity>(Intent.Extras.GetString(BusinessEntityIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                var (oaf, tag) = ObjectActionsFragment.NewInstance(be);
                ft.Replace(Resource.Id.fragment_container, oaf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ObjectActionsActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ObjectActionsActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}