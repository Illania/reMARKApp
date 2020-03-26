using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments.Calendar;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(MainActivity))]
    public class AppointmentActivity : BaseAppCompatActivity
    {
        const string CalendarIdKey = "calendarId";
        const string AppointmentIdKey = "appointmentId";
        const string RecurrenceIndexKey = "recurrenceIndex";

        public static Intent CreateIntent(Context context, int calendarId, int appointmentId, int recurrenceIndex)
        {
            var intent = new Intent(context, typeof(AppointmentActivity));

            intent.PutExtra(CalendarIdKey, calendarId);
            intent.PutExtra(AppointmentIdKey, appointmentId);
            intent.PutExtra(RecurrenceIndexKey, recurrenceIndex);

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(AppointmentActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetContentView(Resource.Layout.base_layout);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var calendarId = Intent.GetIntExtra(CalendarIdKey, 0);
                var appointmentId = Intent.GetIntExtra(AppointmentIdKey, 0);
                var recurrenceIndex = Intent.GetIntExtra(RecurrenceIndexKey, 0);

                var (cf, tag) = AppointmentFragment.NewInstance(calendarId, appointmentId, recurrenceIndex, false);
                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, cf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(AppointmentActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(AppointmentActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }
    }
}
