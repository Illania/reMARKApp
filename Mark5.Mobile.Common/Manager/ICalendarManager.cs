using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Manager
{
    public interface ICalendarManager
    {
        Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(List<int> calendarIds, DateTime startDate, DateTime endDate, SourceType sourceType = SourceType.Auto);

        Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarId, int calendarAppointmentId, SourceType sourceType = SourceType.Auto);

        Task<bool> CreateOrUpdateCalendarAppointmentAsync(int calendarId, CalendarAppointment calendarAppointment, SourceType sourceType = SourceType.Auto);

        Task<bool> SendCalendarAppointmentInvitationsAsync(int apointmentId, Guid lineGuid);

        Task<List<CalendarAlarm>> GetCalendarAlarmsAsync(List<int> calendarIds, long startDateTimeStamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto);
    }
}