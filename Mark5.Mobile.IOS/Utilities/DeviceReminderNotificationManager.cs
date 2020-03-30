using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using UserNotifications;

namespace Mark5.Mobile.IOS.Utilities
{
    public class DeviceReminderNotificationManager : IDeviceReminderNotificationManager
    {
        static readonly string reminderNotificationId = "Reminder";
        static readonly string appointmentIdKey = "appointmentId";
        static readonly string recurrenceIndexKey = "recurrenceIndex";
        static readonly string calendarIdKey = "calendarIdKey";

        readonly UNUserNotificationCenter notificationCenter = UNUserNotificationCenter.Current;

        readonly NSDateFormatter timeFormatter = new NSDateFormatter
        {
            DateStyle = NSDateFormatterStyle.None,
            TimeStyle = NSDateFormatterStyle.Short,
        };

        readonly NSDateFormatter dateFormatter = new NSDateFormatter
        {
            DateStyle = NSDateFormatterStyle.Short,
            TimeStyle = NSDateFormatterStyle.None,
        };

        public void CancelDeviceReminderNotifications(List<CalendarReminder> remindersToCancel)
        {
            notificationCenter.RemoveAllPendingNotificationRequests();
        }

        public void SetDeviceRemindersNotification(List<CalendarReminder> remindersToSet)
        {
            var notificationRequests = remindersToSet.Select(CreateNotificationRequestFromReminder);

            Action<NSError> errorHandler = (NSError error) =>
            {
                if (error != null)
                    CommonConfig.Logger.Error($"Error while sending notification request - {error}");
            };

            foreach (var request in notificationRequests)
                notificationCenter.AddNotificationRequest(request, errorHandler);
        }

        UNNotificationRequest CreateNotificationRequestFromReminder(CalendarReminder reminder)
        {
            var userInfo = new Dictionary<string, string>();
            userInfo.Add(appointmentIdKey, reminder.AppointmentId.ToString());
            userInfo.Add(recurrenceIndexKey, reminder.RecurrenceIndex.ToString());
            userInfo.Add(calendarIdKey, reminder.CalendarId.ToString());

            var userInfoNs = NSDictionary.FromObjectsAndKeys(userInfo.Values.ToArray(), userInfo.Keys.ToArray());

            var notificationContent = new UNMutableNotificationContent
            {
                Title = reminder.Subject,
                Body = FormatDateTime(reminder.StartTime),
                UserInfo = userInfoNs,
                Sound = UNNotificationSound.Default,
            };

            var dateTime = reminder.ReminderTime;
            var dateComponents = GetComponentsFromDateTime(dateTime);

            var trigger = UNCalendarNotificationTrigger.CreateTrigger(dateComponents, false);
            return UNNotificationRequest.FromIdentifier(reminderNotificationId, notificationContent, trigger);
        }

        string FormatDateTime(DateTime dateTime)
        {
            string day;
            string time;

            var components = GetComponentsFromDateTime(dateTime);
            var nsDate = NSCalendar.CurrentCalendar.DateFromComponents(components);

            if (dateTime.Date == DateTime.Today)
                day = "Today";
            else if (dateTime.Date == DateTime.Today.AddDays(1))
                day = "Tomorrow";
            else
                day = dateFormatter.StringFor(nsDate);

            time = timeFormatter.StringFor(nsDate);

            return $"{day} at {time}";
        }

        NSDateComponents GetComponentsFromDateTime(DateTime dateTime)
        {
            return new NSDateComponents
            {
                Day = dateTime.Day,
                Month = dateTime.Month,
                Year = dateTime.Year,
                Hour = dateTime.Hour,
                Minute = dateTime.Minute,
                TimeZone = NSTimeZone.LocalTimeZone
            };
        }

        #region Public methods

        public static ReminderInfo? GetReminderInfo(UNNotification notification)
        {
            if (notification?.Request.Identifier != reminderNotificationId)
                return null;

            var userInfo = notification.Request.Content.UserInfo.ToDictionary<KeyValuePair<NSObject, NSObject>, string, string>(
         item => (NSString)item.Key, item => item.Value.ToString());

            return new ReminderInfo
            {
                AppointmentId = int.Parse(userInfo[appointmentIdKey]),
                RecurrenceIndex = int.Parse(userInfo[recurrenceIndexKey]),
                CalendarId = int.Parse(userInfo[calendarIdKey])
            };
        }

        public struct ReminderInfo
        {
            public int AppointmentId { get; set; }
            public int RecurrenceIndex { get; set; }
            public int CalendarId { get; set; }
        }
        #endregion

    }
}
