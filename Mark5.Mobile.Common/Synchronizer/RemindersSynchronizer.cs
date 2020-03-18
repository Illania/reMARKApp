using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.Synchronizer
{
    public interface ILocalRemindersSynchronizer
    {
        Task Synchronize();
    }

    public class LocalRemindersSynchronizer : ILocalRemindersSynchronizer
    {
        bool initialized;
        List<CalendarReminder> currentReminders;  //Need to be started from DB
        IDeviceReminderNotificationManager deviceReminderNotificationManager = CommonConfig.DeviceReminderNotificationManager;

        public async Task Synchronize()
        {
            try
            {
                await InitializeReminders();

                deviceReminderNotificationManager.CancelDeviceReminderNotifications(currentReminders);

                var nextDayAppointments = await GetNearFutureAppointments();
                var newReminders = ExtractRemindersFromAppointments(nextDayAppointments).ToList();
                await CacheReminders(newReminders);

                if (newReminders.Any())
                    deviceReminderNotificationManager.SetDeviceRemindersNotification(newReminders);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while synchronizing local reminders", ex);
            }
        }

        async Task CacheReminders(List<CalendarReminder> reminders)
        {
            currentReminders = reminders;
            await Managers.CalendarManager.SaveCalendarRemindersAsync(currentReminders);
        }

        async Task InitializeReminders()
        {
            if (!initialized)
            {
                currentReminders = await Managers.CalendarManager.GetCalendarRemindersAsync();
                initialized = true;
            }
        }

        async Task<List<CalendarAppointment>> GetNearFutureAppointments()
        {
            var calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.Select(c => c.Id).ToList();

            try
            {
                var appointments = await Managers.CalendarManager.GetCalendarAppointmentsAsync(calendarsList, DateTime.Now, DateTime.Now.AddDays(8), SourceType.Local);

                return appointments;
            }
            catch (Exception ex)
            {
                if (ex is DataNotFoundException)
                    CommonConfig.Logger.Info("No next day appointment available");
                else
                    CommonConfig.Logger.Error("Error while retrieving next day appointments", ex);

                return new List<CalendarAppointment>();
            }
        }

        List<CalendarReminder> ExtractRemindersFromAppointments(List<CalendarAppointment> appointments)
        {
            var appointmentsWithReminders = appointments.Where(a => a.ReminderTimeBeforeStart > -1);
            var reminders = new List<CalendarReminder>();

            foreach (var app in appointmentsWithReminders)
            {
                if (app.ReminderTimeBeforeStart <= -1)
                    continue;

                foreach (var occurrence in app.Occurrences)
                {
                    var reminder = new CalendarReminder
                    {
                        AppointmentId = app.Id,
                        CalendarId = app.CalendarId,
                        Description = app.Description,
                        AllDay = app.AllDay,  //TODO need to check what happens with this
                        Location = app.Location,
                        RecurrenceIndex = occurrence.RecurrenceIndex,
                        Subject = app.Subject,
                        ReminderTime = occurrence.StartDate.AddSeconds(-app.ReminderTimeBeforeStart),
                        StartTime = occurrence.StartDate,
                    };
                    reminders.Add(reminder);
                }
            }

            return reminders;
        }

        //TODO on ios the identifier for notification is a string, perfect
        //and we can remove all the pending notification request, perfect

        //On Android unfortunately, we can't do a total cancel, but we need to do it one by one, by recreating the pending intent we created before.
        //This would mean that on android we need to keep a list on the sistem of the current pending intents or similar in order to remove them
        //All alarms get cancelled when the phone reboots on Android or when the user changes its date/times from settings. In both cases
        // we need to listen to those events with a broadcast receiver:
        //https://android.jlelse.eu/using-alarmmanager-like-a-pro-20f89f4ca720

    }
}
