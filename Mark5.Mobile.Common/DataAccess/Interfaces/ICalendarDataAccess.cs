using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{
    interface ICalendarDataAccess
    {
        Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarAppointmentId, int recurrenceIndex = -1);

        Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(List<int> calendarIds, DateTime startDate, DateTime endDate);

        Task SaveCalendarAppointmentAsync(CalendarAppointment calendarAppointment);

        Task SaveCalendarAppointmentsAsync(List<int> calendarIds, List<CalendarAppointment> calendarAppointments, DateTime startDate, DateTime endDate);

        Task DeleteAsync(List<CalendarAppointment> appointments);

        Task<List<CalendarAlarm>> GetCalendarAlarmsAsync(List<int> calendarIds, DateTime startDate, DateTime endDate);

        Task SaveCalendarAlarmsAsync(List<int> calendarIds, List<CalendarAlarm> alarms, DateTime startDate, DateTime endDate);

        Task RemoveOrphans();
    }
}