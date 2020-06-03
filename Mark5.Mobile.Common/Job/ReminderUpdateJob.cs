using System;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Synchronizer;

namespace Mark5.Mobile.Common.Job
{
    public interface IRemindersUpdateJob : IJob
    {
    }

    class RemindersUpdateJob : IRemindersUpdateJob
    {
        public async Task Run()
        {
            if (ServerConfig.SystemSettings?.SystemInfo.SystemVersion < new Version(1, 35, 12))
                return;

            try
            {
                var calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.Select(c => c.Id).ToList();
                var start = DateTime.UtcNow;
                var end = start.AddDays(8);

                await Managers.CalendarManager.GetCalendarAppointmentsAsync(calendarsList, start, end, Model.SourceType.Remote);
                await Synchronizers.LocalRemindersSynchronizer.Synchronize();

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while updating local reminders", ex);
            }
        }
    }
}
