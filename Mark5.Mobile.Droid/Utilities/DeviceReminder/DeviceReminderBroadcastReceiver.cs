using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Text.Format;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Utilities.DeviceReminder
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { DeviceReminderNotificationManager.ReminderAction })]
    public class DeviceReminderBroadcastReceiver : BroadcastReceiver
    {
        public static readonly string CalendarChannelId = "calendar";
        static readonly string calendarChannelName = "Calendar";

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != DeviceReminderNotificationManager.ReminderAction)
                return;

            var reminder = Serializer.Deserialize<CalendarReminder>(intent.GetStringExtra(DeviceReminderNotificationManager.ReminderKey));
            var reminderId = $"{reminder.CalendarId}/{reminder.AppointmentId}/{reminder.RecurrenceIndex}";

            var appIntent = WrapperActivity.CreateShowAppointmentIntent(context, reminder.CalendarId, reminder.AppointmentId, reminder.RecurrenceIndex);
            var pendingIntent = PendingIntent.GetActivity(context, 0, appIntent, PendingIntentFlags.OneShot);

            var title = reminder.Subject;
            var date = FormatDateTime(context, reminder.StartTime);

            var nb = new NotificationCompat.Builder(context, CalendarChannelId)
              .SetSmallIcon(Resource.Mipmap.ic_icon)
              .SetColor(ContextCompat.GetColor(context, Resource.Color.darkerblue))
              .SetContentTitle(title).SetContentText(date)
              .SetCategory(Android.App.Notification.CategoryAlarm)
              .SetAutoCancel(true)
              .SetContentIntent(pendingIntent)
              .SetPriority((int)NotificationPriority.High)
              .SetStyle(new NotificationCompat.BigTextStyle()
              .BigText(date));

            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

            notificationManager.Notify(reminderId, 0, nb.Build());
        }

        //TODO check what we did with the other "job" (for system settings
        //TODO create recurring service to download new alarms
        //TODO all alarms get cancelled when phone reboots (create ticket)
        //TODO create ticket for AllDay reminders

        string FormatDateTime(Context context, DateTime dateTime)
        {
            string date;
            string time;

            var timestamp = dateTime.ConvertDateTimeToTimestampMilliseconds();

            if (dateTime.Date == DateTime.Today)
                date = "Today";
            else if (dateTime.Date == DateTime.Today.AddDays(1))
                date = "Tomorrow";
            else
                date = FormatUserTimestampAsDateString(timestamp, context);

            time = FormatUserTimestampAsTimeString(timestamp, context);

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

            channel = new NotificationChannel(CalendarChannelId, calendarChannelName, NotificationImportance.Max);
            notificationManager.CreateNotificationChannel(channel);
#pragma warning restore XA0001 // Find issues with Android API usage
        }
    }
}
