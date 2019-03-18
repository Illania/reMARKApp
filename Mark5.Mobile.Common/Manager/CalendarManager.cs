using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Model.Exceptions;

namespace Mark5.Mobile.Common.Manager
{
    class CalendarManager : AbstractManager, ICalendarManager
    {
        readonly ICalendarDataAccess calendarDataAccess;

        public CalendarManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, ICalendarDataAccess calendarDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.calendarDataAccess = calendarDataAccess;
        }

        public async Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(List<int> calendarIds, DateTime startDate, DateTime endDate, SourceType sourceType = SourceType.Auto)
        {
            var startDateUTC = startDate.ConvertUserTimeToUtc();
            var endDateUTC = endDate.ConvertUserTimeToUtc();

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarAppointmentsAsync(new DataContract.GetCalendarAppointmentsParameters
                {
                    Token = Token,
                    CalendarIds = calendarIds,
                    StartDate = startDateUTC,
                    EndDate = endDateUTC,
                });

                var appointments = result.CalendarAppointments.WhereNotNull().Select(a => a.Convert()).ToList();

                await calendarDataAccess.SaveCalendarAppointmentsAsync(calendarIds, appointments,
                    startDateUTC.ConvertDateTimeToTimestampMilliseconds(), endDateUTC.ConvertDateTimeToTimestampMilliseconds()); //TODO this also should be changed to datetime...

                return appointments;
            }

            if (sourceType == SourceType.Local)
                return await calendarDataAccess.GetCalendarAppointmentsAsync(calendarIds,
                    startDateUTC.ConvertDateTimeToTimestampMilliseconds(), endDateUTC.ConvertDateTimeToTimestampMilliseconds()); //TODO same as up

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarId, int calendarAppointmentId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarAppointmentAsync(new DataContract.GetCalendarAppointmentParameters
                {
                    Token = Token,
                    CalendarId = calendarId,
                    CalendarAppointmentId = calendarAppointmentId
                });

                var appointment = result.CalendarAppointment.Convert();

                await calendarDataAccess.SaveCalendarAppointmentAsync(appointment);

                return appointment;
            }

            if (sourceType == SourceType.Local)
                return await calendarDataAccess.GetCalendarAppointmentAsync(calendarAppointmentId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> CreateOrUpdateCalendarAppointmentAsync(int calendarId, CalendarAppointment calendarAppointment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.CreateOrUpdateCalendarAppointmentAsync(new DataContract.CreateOrUpdateCalendarAppointmentParameters
                {
                    Token = Token,
                    CalendarId = calendarId,
                    CalendarAppointment = calendarAppointment.Convert()
                });

                calendarAppointment.Id = result.Id;
                calendarAppointment.Guid = result.Guid;

                await calendarDataAccess.SaveCalendarAppointmentAsync(calendarAppointment);

                return result.Updated;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> SendCalendarAppointmentInvitationsAsync(int appointmentId, Guid lineGuid)
        {
            var result = await AppServiceProxy.SendCalendarAppointmentInvitationsAsync(new DataContract.SendCalendarAppointmentInvitationsParameters
            {
                Token = Token,
                LineGuid = lineGuid,
                CalendarAppointmentId = appointmentId
            });

            return true;
        }

        public async Task<List<CalendarAlarm>> GetCalendarAlarmsAsync(List<int> calendarIds, long startDateTimestamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarAlarms(new DataContract.GetCalendarAlarmsParameters
                {
                    Token = Token,
                    CalendarIds = calendarIds,
                    StartDate = startDateTimestamp.ConvertTimestampMillisecondsToDateTime(),
                    EndDate = endDateTimestamp.ConvertTimestampMillisecondsToDateTime(),
                });

                var alarms = result.Alarms.WhereNotNull().Select(a => a.Convert()).ToList();

                await calendarDataAccess.SaveCalendarAlarmsAsync(calendarIds, alarms, startDateTimestamp, endDateTimestamp);

                return alarms;
            }

            if (sourceType == SourceType.Local)
                return await calendarDataAccess.GetCalendarAlarmsAsync(calendarIds, startDateTimestamp, endDateTimestamp);

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}