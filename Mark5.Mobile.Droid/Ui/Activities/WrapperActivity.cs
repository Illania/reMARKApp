using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    //This activity is used just to decide which other activity to open according to the status.
    [Activity(NoHistory = true)]
    public class WrapperActivity : AppCompatActivity
    {
        const string CalendarIdKey = "calendarId";
        const string AppointmentIdKey = "appointmentId";
        const string RecurrenceIndexKey = "recurrenceIndex";

        public static Intent CreateShowAppointmentIntent(Context context, int calendarId, int appointmentId, int recurrenceIndex)
        {
            var intent = new Intent(context, typeof(WrapperActivity));

            intent.PutExtra(CalendarIdKey, calendarId);
            intent.PutExtra(AppointmentIdKey, appointmentId);
            intent.PutExtra(RecurrenceIndexKey, recurrenceIndex);

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var calendarId = Intent.GetIntExtra(CalendarIdKey, 0);
            var appointmentId = Intent.GetIntExtra(AppointmentIdKey, 0);
            var recurrenceIndex = Intent.GetIntExtra(RecurrenceIndexKey, 0);

            Intent intent;

            if (((Mark5Application)ApplicationContext).StartedFromRoot)
                intent = AppointmentActivity.CreateIntent(this, calendarId, appointmentId, recurrenceIndex);
            else
                intent = SplashActivity.CreateShowAppointmentIntent(this, calendarId, appointmentId, recurrenceIndex);

            StartActivity(intent);
            Finish();
        }

    }
}
