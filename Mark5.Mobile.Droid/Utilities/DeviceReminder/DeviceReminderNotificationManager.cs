using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities.DeviceReminder
{
    public class DeviceReminderNotificationManager : IDeviceReminderNotificationManager
    {
        readonly AlarmManager alarmManager = Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
        public const string ReminderKey = "reminderKey";
        public const string ReminderAction = "reminderAction";

        static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        public void CancelDeviceReminderNotifications(List<CalendarReminder> remindersToCancel)
        {
            foreach (var reminder in remindersToCancel)
                alarmManager.Cancel(GetPendingIntentForReminder(reminder));
        }

        public void SetDeviceRemindersNotification(List<CalendarReminder> remindersToSet)
        {
            foreach (var reminder in remindersToSet)
                SetAlarmForReminder(reminder);
        }

        void SetAlarmForReminder(CalendarReminder reminder)
        {
            var pi = GetPendingIntentForReminder(reminder);

            var alarmTime = (long)(reminder.ReminderTime.ToUniversalTime() - epoch).TotalMilliseconds;

            alarmManager.SetExact(AlarmType.RtcWakeup, alarmTime, pi);
        }

        PendingIntent GetPendingIntentForReminder(CalendarReminder reminder)
        {
            var reminderId = $"{reminder.CalendarId}/{reminder.AppointmentId}/{reminder.RecurrenceIndex}/{reminder.ReminderTime.Ticks}";
            var uri = Android.Net.Uri.Parse("reminder://" + reminderId);
            var intent = new Intent(Application.Context, typeof(DeviceReminderBroadcastReceiver));
            intent.SetData(uri);
            intent.SetAction(ReminderAction);
            intent.PutExtra(ReminderKey, Serializer.Serialize(reminder));

            return PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.CancelCurrent);
        }
    }
}
