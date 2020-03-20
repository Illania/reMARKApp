using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Text.Format;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities.DeviceReminder
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { DeviceReminderNotificationManager.ReminderAction })]
    public class DeviceReminderBroadcastReceiver : BroadcastReceiver
    {
        bool registered;

        public static readonly string CalendarChannelId = "calendar";
        static readonly string calendarChannelName = "Calendar";

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != DeviceReminderNotificationManager.ReminderAction)
                return;

            var reminder = Serializer.Deserialize<CalendarReminder>(intent.GetStringExtra(DeviceReminderNotificationManager.ReminderKey));


            var title = reminder.Subject;
            var d = reminder.StartTime.ConvertDateTimeToTimestampMilliseconds();
            var date = d.FormatUserTimestampAsCompactMediumDateTimeString(context);

            //TODO need to put correct timing
            //TODO need to add channel creation
            //TODO need to add intent when clicking
            var nb = new NotificationCompat.Builder(Application.Context, CalendarChannelId)
              .SetSmallIcon(Resource.Mipmap.ic_icon)
              .SetColor(ContextCompat.GetColor(Application.Context, Resource.Color.darkerblue))
              .SetContentTitle(title).SetContentText(date)
              .SetAutoCancel(true)
              .SetPriority((int)NotificationPriority.High)
              .SetStyle(new NotificationCompat.BigTextStyle()
              .BigText(date));

            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

            notificationManager.Notify(0, nb.Build()); //TODO need to change id 
        }

        string FormatDateTime(DateTime dateTime)
        {
            string date = null;
            string time = null;

            var timestamp = dateTime.ConvertDateTimeToTimestampMilliseconds();

            if (dateTime.Date == DateTime.Today)
                date = "Today";
            else if (dateTime.Date == DateTime.Today.AddDays(1))
                date = "Tomorrow";
            else
                date = FormatUserTimestampAsDateString(timestamp, Application.Context); //TODO maybe context can be moved

            time = FormatUserTimestampAsTimeString(timestamp, Application.Context);

            return $"{date} at {time}";
        }

        string FormatUserTimestampAsTimeString(long timestamp, Context context)
        {
            var date = new Java.Util.Date(timestamp);
            var tf = DateFormat.GetTimeFormat(context);
            tf.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            return tf.Format(date);
        }

        string FormatUserTimestampAsDateString(long timestamp, Context context)
        {
            var date = new Java.Util.Date(timestamp);
            var df = DateFormat.GetDateFormat(context);
            df.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            return df.Format(date);
        }

        public static void CreateChannelIfNotExists(Context context)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            NotificationManager notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

#pragma warning disable XA0001 // Find issues with Android API usage
            NotificationChannel channel = notificationManager.GetNotificationChannel(CalendarChannelId);
            if (channel != null)
                return;

            channel = new NotificationChannel(CalendarChannelId, calendarChannelName, NotificationImportance.High);
            notificationManager.CreateNotificationChannel(channel);
#pragma warning restore XA0001 // Find issues with Android API usage
        }
    }
}
