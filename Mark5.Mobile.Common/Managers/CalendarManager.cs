//
// Project: Mark5.Mobile.Common
// File: CalendarManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
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

namespace Mark5.Mobile.Common.Managers
{
    class CalendarManager : AbstractManager, ICalendarManager
    {

        readonly ICalendarDataAccess calendarDataAccess;

        public CalendarManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, ICalendarDataAccess calendarDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.calendarDataAccess = calendarDataAccess;
        }

        public async Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(Folder folder, long startDateTimestamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                //Eventually add check on folder (also following function)
                var result = await AppServiceProxy.GetCalendarEventsAsync(new DataContract.GetCalendarEventsParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    GetAppointments = true,
                    GetTasks = false,
                    StartDate = startDateTimestamp.ConvertTimestampMillisecondsToDateTime(),
                    EndDate = endDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                });

                var appointments = result.CalendarAppointments.WhereNotNull().Select(a => a.Convert()).ToList();

                await calendarDataAccess.SaveCalendarAppointmentsAsync(folder, appointments);

                return appointments;
            }

            if (sourceType == SourceType.Local)
            {
                List<CalendarAppointment> appointments = null;

                appointments = await calendarDataAccess.GetCalendarAppointmentsAsync(folder, startDateTimestamp, endDateTimestamp);

                return appointments;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<CalendarTask>> GetCalendarTasksAsync(Folder folder, long startDateTimestamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarEventsAsync(new DataContract.GetCalendarEventsParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    GetAppointments = false,
                    GetTasks = true,
                    StartDate = startDateTimestamp.ConvertTimestampMillisecondsToDateTime(),
                    EndDate = endDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                });

                var tasks = result.CalendarTasks.WhereNotNull().Select(t => t.Convert()).ToList();

                await calendarDataAccess.SaveCalendarTasksAsync(folder, tasks);

                return tasks;
            }

            if (sourceType == SourceType.Local)
            {
                List<CalendarTask> tasks = null;

                tasks = await calendarDataAccess.GetCalendarTasksAsync(folder, startDateTimestamp, endDateTimestamp);

                return tasks;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<CalendarAppointment> GetCalendarAppointmentAsync(Folder folder, int calendarAppointmentId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarAppointmentAsync(new DataContract.GetCalendarAppointmentParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    CalendarAppointmentId = calendarAppointmentId
                });

                var appointment = result.CalendarAppointment.Convert();

                await calendarDataAccess.SaveCalendarAppointmentAsync(appointment);

                return appointment;
            }

            if (sourceType == SourceType.Local)
            {
                return await calendarDataAccess.GetCalendarAppointmentAsync(calendarAppointmentId);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<CalendarTask> GetCalendarTaskAsync(Folder folder, int calendarTaskId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarTaskAsync(new DataContract.GetCalendarTaskParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    CalendarTaskId = calendarTaskId
                });

                var task = result.CalendarTask.Convert();

                await calendarDataAccess.SaveCalendarTaskAsync(task);

                return task;
            }

            if (sourceType == SourceType.Local)
            {
                return await calendarDataAccess.GetCalendarTaskAsync(calendarTaskId);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> CreateOrUpdateCalendarAppointmentAsync(Folder folder, CalendarAppointment calendarAppointment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.CreateOrUpdateCalendarAppointmentAsync(new DataContract.CreateOrUpdateCalendarAppointmentParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    CalendarAppointment = calendarAppointment.Convert()
                });

                calendarAppointment.Id = result.Id;
                calendarAppointment.Guid = result.Guid;

                return result.Updated;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> CreateOrUpdateCalendarTaskAsync(Folder folder, CalendarTask calendarTask, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.CreateOrUpdateCalendarTaskAsync(new DataContract.CreateOrUpdateCalendarTaskParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    CalendarTask = calendarTask.Convert()
                });

                calendarTask.Id = result.Id;
                calendarTask.Guid = result.Guid;

                return result.Updated;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

    }
}

