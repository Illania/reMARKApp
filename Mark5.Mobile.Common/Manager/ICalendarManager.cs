using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Manager
{
    public interface ICalendarManager
    {
        Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(List<int> calendarIds, long startDateTimestamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto);

        Task<List<CalendarTask>> GetCalendarTasksAsync(List<int> calendarIds, long startDateTimestamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto);

        Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarId, int calendarAppointmentId, SourceType sourceType = SourceType.Auto);

        Task<CalendarTask> GetCalendarTaskAsync(int calendarId, int calendarTaskId, SourceType sourceType = SourceType.Auto);

        Task<bool> CreateOrUpdateCalendarAppointmentAsync(int calendarId, CalendarAppointment calendarAppointment, SourceType sourceType = SourceType.Auto);

        Task<bool> CreateOrUpdateCalendarTaskAsync(int calendarId, CalendarTask calendarTask, SourceType sourceType = SourceType.Auto);
    }
}