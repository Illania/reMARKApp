//
// Project: Mark5.Mobile.Common
// File: ICalendarDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{

    interface ICalendarDataAccess
    {
        Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarAppointmentId);

        Task<CalendarTask> GetCalendarTaskAsync(int calendarTaskId);

        Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(Folder folder, long startDateTimestamp, long endDateTimestamp);

        Task<List<CalendarTask>> GetCalendarTasksAsync(Folder folder, long startDateTimestamp, long endDateTimestamp);

        Task SaveCalendarAppointmentAsync(CalendarAppointment calendarAppointment);

        Task SaveCalendarTaskAsync(CalendarTask calendarTask);

        Task SaveCalendarAppointmentsAsync(Folder folder, IEnumerable<CalendarAppointment> calendarAppointments, bool clean = false);

        Task SaveCalendarTasksAsync(Folder folder, IEnumerable<CalendarTask> calendarTasks, bool clean = false);

        Task RemoveFromFolderAsync(List<CalendarAppointment> appointments, Folder folder);

        Task RemoveFromFolderAsync(List<CalendarTask> tasks, Folder folder);

        Task DeleteAsync(List<CalendarAppointment> appointments);

        Task DeleteAsync(List<CalendarTask> tasks);

        Task RemoveOrphans();
    }
}

