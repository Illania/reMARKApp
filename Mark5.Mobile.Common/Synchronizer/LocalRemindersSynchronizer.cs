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

    class LocalRemindersSynchronizer : ILocalRemindersSynchronizer
    {
        bool initialized;
        List<CalendarReminder> currentReminders;
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
                    var reminderTime = occurrence.StartDate.AddSeconds(-app.ReminderTimeBeforeStart);

                    if (reminderTime < DateTime.Now.AddMinutes(-5))
                        continue;

                    var reminder = new CalendarReminder
                    {
                        AppointmentId = app.Id,
                        CalendarId = app.CalendarId,
                        Description = app.Description,
                        AllDay = app.AllDay,
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
    }
}
