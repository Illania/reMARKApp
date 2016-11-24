//
// Project: Mark5.Mobile.Common
// File: ICalendarManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Managers
{

    public interface ICalendarManager
    {

        Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(Folder folder, long startDateTimestamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto);

        Task<List<CalendarTask>> GetCalendarTasksAsync(Folder folder, long startDateTimestamp, long endDateTimestamp, SourceType sourceType = SourceType.Auto);

        Task<CalendarAppointment> GetCalendarAppointmentAsync(Folder folder, int calendarAppointmentId, SourceType sourceType = SourceType.Auto);

        Task<CalendarTask> GetCalendarTaskAsync(Folder folder, int calendarTaskId, SourceType sourceType = SourceType.Auto);

        Task<bool> CreateOrUpdateCalendarAppointmentAsync(Folder folder, CalendarAppointment calendarAppointment, SourceType sourceType = SourceType.Auto);

        Task<bool> CreateOrUpdateCalendarTaskAsync(Folder folder, CalendarTask calendarTask, SourceType sourceType = SourceType.Auto);
    }
}

