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

        public async Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(List<int> calendarIds, long startDateTimestamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarEventsAsync(new DataContract.GetCalendarEventsParameters
                {
                    Token = Token,
                    CalendarIds = calendarIds,
                    GetAppointments = true,
                    GetTasks = false,
                    StartDate = startDateTimestamp.ConvertTimestampMillisecondsToDateTime(),
                    EndDate = endDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                });

                var appointments = result.CalendarAppointments.WhereNotNull().Select(a => a.Convert()).ToList();

                //TODO await calendarDataAccess.SaveCalendarAppointmentsAsync(folder, appointments);

                return appointments;
            }

            if (sourceType == SourceType.Local)
            {
                List<CalendarAppointment> appointments = null;

                //TODO  appointments = await calendarDataAccess.GetCalendarAppointmentsAsync(folder, startDateTimestamp, endDateTimestamp);

                return appointments;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<CalendarTask>> GetCalendarTasksAsync(List<int> calendarIds, long startDateTimestamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarEventsAsync(new DataContract.GetCalendarEventsParameters
                {
                    Token = Token,
                    CalendarIds = calendarIds,
                    GetAppointments = false,
                    GetTasks = true,
                    StartDate = startDateTimestamp.ConvertTimestampMillisecondsToDateTime(),
                    EndDate = endDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                });

                var tasks = result.CalendarTasks.WhereNotNull().Select(t => t.Convert()).ToList();

                //TODO  await calendarDataAccess.SaveCalendarTasksAsync(folder, tasks);

                return tasks;
            }

            if (sourceType == SourceType.Local)
            {
                List<CalendarTask> tasks = null;

                //TODO  tasks = await calendarDataAccess.GetCalendarTasksAsync(folder, startDateTimestamp, endDateTimestamp);

                return tasks;
            }

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

                //TODO await calendarDataAccess.SaveCalendarAppointmentAsync(appointment);

                return appointment;
            }

            if (sourceType == SourceType.Local)
                return await calendarDataAccess.GetCalendarAppointmentAsync(calendarAppointmentId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<CalendarTask> GetCalendarTaskAsync(int calendarId, int calendarTaskId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarTaskAsync(new DataContract.GetCalendarTaskParameters
                {
                    Token = Token,
                    CalendarId = calendarId,
                    CalendarTaskId = calendarTaskId
                });

                var task = result.CalendarTask.Convert();

                //TODO await calendarDataAccess.SaveCalendarTaskAsync(task);

                return task;
            }

            if (sourceType == SourceType.Local)
                return await calendarDataAccess.GetCalendarTaskAsync(calendarTaskId);

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

                return result.Updated;
            }  //TODO save to db

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> CreateOrUpdateCalendarTaskAsync(int calendarId, CalendarTask calendarTask, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.CreateOrUpdateCalendarTaskAsync(new DataContract.CreateOrUpdateCalendarTaskParameters
                {
                    Token = Token,
                    CalendarId = calendarId,
                    CalendarTask = calendarTask.Convert()
                });

                calendarTask.Id = result.Id;
                calendarTask.Guid = result.Guid;

                return result.Updated;
            }  //TODO save to db

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

                //TODO

                return alarms;
            }

            if (sourceType == SourceType.Local)
            {
                List<CalendarAlarm> alarms = null;

                //TODO 

                return alarms;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}