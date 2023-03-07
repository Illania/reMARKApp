using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class TransmitDestinationsListActivity : BaseAppCompatActivity
    {
        private const string DocumentIdBundleKey = "DocumentId_d3ded4d4-be9a-49e6-8626-84cb175c12bb";
        private const string ReferenceNumberBundleKey = "ReferenceNumber_40876832-91a3-46d7-a57e-6d850847c295";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, int? documentId, string referenceNumber)
        {
            var intent = new Intent(context, typeof(TransmitDestinationsListActivity));

            if (documentId != null)
                intent.PutExtra(DocumentIdBundleKey, documentId.Value);

            if (referenceNumber != null)
                intent.PutExtra(ReferenceNumberBundleKey, referenceNumber);

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(TransmitDestinationsListActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                int? documentId = null;
                string referenceNumber = null;

                if (Intent.HasExtra(ReferenceNumberBundleKey))
                    referenceNumber = Intent.Extras.GetString(ReferenceNumberBundleKey);

                if (Intent.HasExtra(DocumentIdBundleKey))
                    documentId = Intent.Extras.GetInt(DocumentIdBundleKey);


                var (cf, tag) = TransmitDestinationsListFragment.NewInstance(documentId, referenceNumber);
                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, cf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(DeliveryReportActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(DeliveryReportActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }
    }
}
  