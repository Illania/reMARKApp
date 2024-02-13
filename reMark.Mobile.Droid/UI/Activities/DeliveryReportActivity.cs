using System;
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
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class DeliveryReportActivity : BaseAppCompatActivity
    {
        const string TransmitDestinationBundleKey = "TransmitDestinationId_da4826eb-eb7a-4ceb-bd12-9c735bef1555";
        const string ReferenceNumberBundleKey = "ReferenceNumber_40876832-91a3-46d7-a57e-6d850847c295";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, TransmitDestination destination, string referenceNumber)
        {
            var intent = new Intent(context, typeof(DeliveryReportActivity));

            if (destination != null)
                intent.PutExtra(TransmitDestinationBundleKey, Serializer.Serialize(destination));

            if (referenceNumber != null)
                intent.PutExtra(ReferenceNumberBundleKey,referenceNumber);

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DeliveryReportActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                TransmitDestination transmitDestination = null;
                string referenceNumber = null;

                if (Intent.HasExtra(ReferenceNumberBundleKey))
                    referenceNumber = Intent.Extras.GetString(ReferenceNumberBundleKey);

                if (Intent.HasExtra(TransmitDestinationBundleKey))
                    transmitDestination = Serializer.Deserialize<TransmitDestination>(Intent.Extras.GetString(TransmitDestinationBundleKey));


                var (cf, tag) = DeliveryReportFragment.NewInstance(transmitDestination, referenceNumber);
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
