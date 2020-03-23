using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Java.Lang;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities.DeviceReminder
{
    public class DeviceReminderNotificationManager : IDeviceReminderNotificationManager
    {
        AlarmManager alarmManager = Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
        public const string ReminderKey = "reminderKey";
        public const string ReminderAction = "reminderAction";

        static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        public void CancelDeviceReminderNotifications(List<CalendarReminder> remindersToCancel)
        {
            //TODO later
        }

        public void SetDeviceRemindersNotification(List<CalendarReminder> remindersToSet)
        {
            foreach (var reminder in remindersToSet)
            {
                SetAlarmForReminder(reminder);
            }
        }

        void SetAlarmForReminder(CalendarReminder reminder)
        {
            var pi = GetPendingIntentForReminder(reminder);

            var alarmTime = (long)(reminder.ReminderTime.ToUniversalTime() - epoch).TotalMilliseconds; //TODO need to check if correct

            var alarmTime2 = JavaSystem.CurrentTimeMillis() + 15 * 100;
            alarmManager.SetExact(AlarmType.RtcWakeup, alarmTime2, pi);
        }

        PendingIntent GetPendingIntentForReminder(CalendarReminder reminder)
        {
            var intent = new Intent(Application.Context, typeof(DeviceReminderBroadcastReceiver));
            intent.SetAction(ReminderAction);
            intent.PutExtra(ReminderKey, Serializer.Serialize(reminder));

            return PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.CancelCurrent);
        }
    }
}
