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
                CommonConfig.Logger.Error($"Error while sending notification request - {error}");
            };

            foreach (var request in notificationRequests)
                notificationCenter.AddNotificationRequest(request, errorHandler);
        }

        UNNotificationRequest CreateNotificationRequestFromReminder(CalendarReminder reminder)
        {
            var notificationContent = new UNMutableNotificationContent
            {
                Title = reminder.Subject,
                Body = FormatDateTime(reminder.StartTime)
            };

            var dateTime = reminder.ReminderTime;
            var dateComponents = GetComponentsFromDateTime(dateTime);

            var trigger = UNCalendarNotificationTrigger.CreateTrigger(dateComponents, false);

            var identifier = $"calendar/{reminder.CalendarId}/{reminder.RecurrenceIndex}";
            return UNNotificationRequest.FromIdentifier(identifier, notificationContent, trigger);
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
    }
}
