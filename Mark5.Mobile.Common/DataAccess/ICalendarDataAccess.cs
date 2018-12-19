using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{
    interface ICalendarDataAccess
    {
        Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarAppointmentId);

        Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(Folder folder, long startDateTimestamp, long endDateTimestamp);

        Task SaveCalendarAppointmentAsync(CalendarAppointment calendarAppointment);

        Task SaveCalendarAppointmentsAsync(Folder folder, IEnumerable<CalendarAppointment> calendarAppointments, bool clean = false);

        Task DeleteAsync(List<CalendarAppointment> appointments);

        Task RemoveOrphans();
    }
}