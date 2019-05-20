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

        Task<List<CalendarAlarm>> GetCalendarAlarmsAsync(List<int> calendarIds, DateTime startDate, DateTime endDate, SourceType sourceType = SourceType.Auto);

        IAppointmentsCache AppointmentsCache { get; }
    }

    public interface IAppointmentsCache
    {
        event EventHandler<AppointmentsRetrievedEventArgs> AppointmentRetrieved;
        event EventHandler<Exception> RetrievalError;

        void GetAppointments(List<int> calendarIds, DateTime startDate, DateTime endDate);

        void Clean();
    }

    public class AppointmentsRetrievedEventArgs : EventArgs
    {
        public IEnumerable<CalendarAppointment> Appointments { get; }
        public DateTime Start { get; }
        public DateTime End { get; }

        public AppointmentsRetrievedEventArgs(IEnumerable<CalendarAppointment> appointments, DateTime startPeriod, DateTime endPeriod)
        {
            Appointments = appointments;
            Start = startPeriod;
            End = endPeriod;
        }
    }

}