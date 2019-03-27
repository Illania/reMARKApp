using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{
    interface ICalendarDataAccess
    {
        Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarAppointmentId);

        Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(List<int> calendarIds, long startDateTimestamp, long endDateTimestamp);

        Task SaveCalendarAppointmentAsync(CalendarAppointment calendarAppointment);

        Task SaveCalendarAppointmentsAsync(List<int> calendarIds, List<CalendarAppointment> calendarAppointments, long startDateTimestamp, long endDateTimestamp);

        Task DeleteAsync(List<CalendarAppointment> appointments);

        Task<List<CalendarAlarm>> GetCalendarAlarmsAsync(List<int> calendarIds, long startDateTimestamp, long endDateTimestamp);

        Task SaveCalendarAlarmsAsync(List<int> calendarIds, List<CalendarAlarm> alarms, long startDateTimestamp, long endDateTimestamp);

        Task RemoveOrphans();
    }
}