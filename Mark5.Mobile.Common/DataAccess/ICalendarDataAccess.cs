using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{
    interface ICalendarDataAccess
    {
        Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarAppointmentId);

        Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(IEnumerable<int> calendarIds, long startDateTimestamp, long endDateTimestamp);

        Task SaveCalendarAppointmentAsync(CalendarAppointment calendarAppointment);

        Task SaveCalendarAppointmentsAsync(IEnumerable<CalendarAppointment> calendarAppointments, long startDateTimestamp, long endDateTimestamp);

        Task DeleteAsync(List<CalendarAppointment> appointments);

        Task RemoveOrphans();
    }
}