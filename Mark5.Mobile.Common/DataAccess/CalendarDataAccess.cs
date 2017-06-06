//
// File: CalendarDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Links;

namespace Mark5.Mobile.Common.DataAccess
{
    class CalendarDataAccess : ICalendarDataAccess
    {
        readonly DatabaseConnectionProvider calendarDatabase;

        public CalendarDataAccess(DatabaseConnectionProvider calendarDatabase)
        {
            this.calendarDatabase = calendarDatabase;
        }

        public async Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarAppointmentId)
        {
            try
            {
                CalendarAppointment appointment = null;

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var result = c.Find<CalendarAppointment>(calendarAppointmentId);
                    appointment = result ?? throw new DataNotFoundException("Appointment could not be found");
                });

                return appointment;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting appointment.", ex);
            }
        }

        public async Task<CalendarTask> GetCalendarTaskAsync(int calendarTaskId)
        {
            try
            {
                CalendarTask appointment = null;

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var result = c.Find<CalendarTask>(calendarTaskId);
                    appointment = result ?? throw new DataNotFoundException("Task could not be found");
                });

                return appointment;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting task.", ex);
            }
        }

        public async Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(Folder folder, long startDateTimestamp = -1, long endDateTimestamp = -1)
        {
            try
            {
                List<CalendarAppointment> appointments = null;

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select * " + $"from {nameof(CalendarAppointment)}, {nameof(FolderCalendarAppointmentLink)} " + $"where {nameof(FolderCalendarAppointmentLink.FolderId)} = {folder.Id} " + $"     and {nameof(CalendarAppointment)}.{nameof(CalendarAppointment.Id)} = {nameof(FolderCalendarAppointmentLink)}.{nameof(FolderCalendarAppointmentLink.CalendarAppointmentId)} ";

                    if (startDateTimestamp >= 0)
                    {
                        query += $"    and {nameof(CalendarAppointment)}.{nameof(CalendarAppointment.StartDateTimestamp)} >= {startDateTimestamp} ";
                    }
                    if (endDateTimestamp >= 0)
                    {
                        query += $"    and {nameof(CalendarAppointment)}.{nameof(CalendarAppointment.EndDateTimestamp)} <= {endDateTimestamp} ";
                    }
                    query += $"order by {nameof(CalendarAppointment.StartDateTimestamp)} desc ";

                    var result = c.Query<CalendarAppointment>(query);

                    if (result == null || result.Count < 1)
                    {
                        throw new DataNotFoundException("Calendar appointments could not be found.");
                    }
                    appointments = result;
                });

                return appointments;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting calendar appointments.", ex);
            }
        }

        public async Task<List<CalendarTask>> GetCalendarTasksAsync(Folder folder, long startDateTimestamp, long endDateTimestamp)
        {
            try
            {
                List<CalendarTask> tasks = null;

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var query = $"select * " + $"from {nameof(CalendarTask)}, {nameof(FolderCalendarTaskLink)} " + $"where {nameof(FolderCalendarTaskLink.FolderId)} = {folder.Id} " + $"     and {nameof(CalendarTask)}.{nameof(CalendarTask.Id)} = {nameof(FolderCalendarTaskLink)}.{nameof(FolderCalendarTaskLink.CalendarTaskId)} ";
                    if (startDateTimestamp >= 0)
                    {
                        query += $"    and {nameof(CalendarTask)}.{nameof(CalendarTask.StartDateTimestamp)} >= {startDateTimestamp} ";
                    }
                    if (endDateTimestamp >= 0)
                    {
                        query += $"    and {nameof(CalendarTask)}.{nameof(CalendarTask.EndDateTimestamp)} <= {endDateTimestamp} ";
                    }
                    query += $"order by {nameof(CalendarTask.StartDateTimestamp)} desc ";


                    var result = c.Query<CalendarTask>(query);

                    if (result == null || result.Count < 1)
                    {
                        throw new DataNotFoundException("Calendar tasks could not be found.");
                    }
                    tasks = result;
                });

                return tasks;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting calendar tasks.", ex);
            }
        }

        public async Task SaveCalendarAppointmentAsync(CalendarAppointment calendarAppointment)
        {
            try
            {
                await calendarDatabase.RunInConnectionAsync(c => { c.InsertOrReplace(calendarAppointment); });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving calendar appointment.", ex);
            }
        }

        public async Task SaveCalendarTaskAsync(CalendarTask calendarTask)
        {
            try
            {
                await calendarDatabase.RunInConnectionAsync(c => { c.InsertOrReplace(calendarTask); });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving calendar task.", ex);
            }
        }

        public async Task SaveCalendarAppointmentsAsync(Folder folder, IEnumerable<CalendarAppointment> calendarAppointments, bool clean = false)
        {
            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    if (clean)
                    {
                        c.Table<FolderCalendarAppointmentLink>().Delete(fdl => fdl.FolderId == folder.Id);
                    }
                    c.InsertOrReplaceAll(calendarAppointments.Select(ca => new FolderCalendarAppointmentLink
                    {
                        FolderId = folder.Id,
                        CalendarAppointmentId = ca.Id
                    }));
                    c.InsertOrReplaceAll(calendarAppointments);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving calendar appointments.", ex);
            }
        }

        public async Task SaveCalendarTasksAsync(Folder folder, IEnumerable<CalendarTask> calendarTasks, bool clean = false)
        {
            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    if (clean)
                    {
                        c.Table<FolderCalendarTaskLink>().Delete(fdl => fdl.FolderId == folder.Id);
                    }
                    c.InsertOrReplaceAll(calendarTasks.Select(ct => new FolderCalendarTaskLink
                    {
                        FolderId = folder.Id,
                        CalendarTaskId = ct.Id
                    }));
                    c.InsertOrReplaceAll(calendarTasks);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving calendar tasks.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<CalendarAppointment> appointments, Folder folder)
        {
            var ids = appointments.Select(t => t.Id).Distinct().ToList();

            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var id in ids)
                    {
                        var linksCount = c.Table<FolderCalendarAppointmentLink>().Count(fdl => fdl.CalendarAppointmentId == id);
                        if (linksCount == 1)
                        {
                            c.Table<CalendarAppointment>().Delete(ct => ct.Id == id);
                        }
                        c.Table<FolderCalendarAppointmentLink>().Delete(fsl => fsl.CalendarAppointmentId == id && fsl.FolderId == folder.Id);
                    }
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing appointments from folder.", ex);
            }
        }

        public async Task RemoveFromFolderAsync(List<CalendarTask> tasks, Folder folder)
        {
            var ids = tasks.Select(t => t.Id).Distinct().ToList();

            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    foreach (var id in ids)
                    {
                        var linksCount = c.Table<FolderCalendarTaskLink>().Count(fdl => fdl.CalendarTaskId == id);
                        if (linksCount == 1)
                        {
                            c.Table<CalendarTask>().Delete(ct => ct.Id == id);
                        }
                        c.Table<FolderCalendarTaskLink>().Delete(fsl => fsl.CalendarTaskId == id && fsl.FolderId == folder.Id);
                    }
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing tasks from folder.", ex);
            }
        }

        public async Task DeleteAsync(List<CalendarAppointment> apppointments)
        {
            var ids = apppointments.Select(a => a.Id).Distinct().ToList();

            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    c.Table<FolderCalendarAppointmentLink>().Delete(fsl => ids.Contains(fsl.CalendarAppointmentId));
                    c.Table<CalendarAppointment>().Delete(ca => ids.Contains(ca.Id));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting appointments.", ex);
            }
        }

        public async Task DeleteAsync(List<CalendarTask> tasks)
        {
            var ids = tasks.Select(t => t.Id).Distinct().ToList();

            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    c.Table<FolderCalendarTaskLink>().Delete(fsl => ids.Contains(fsl.CalendarTaskId));
                    c.Table<CalendarTask>().Delete(ct => ids.Contains(ct.Id));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting tasks.", ex);
            }
        }

        public async Task RemoveOrphans()
        {
            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var innerSelectQueryTextTask = $"select {nameof(FolderCalendarTaskLink.CalendarTaskId)} from {nameof(FolderCalendarTaskLink)}";
                    var outerDeleteQueryTask = $"delete from {nameof(CalendarTask)} where {nameof(CalendarTask.Id)} not in ({innerSelectQueryTextTask}) ";
                    var cmd = c.CreateCommand(outerDeleteQueryTask);
                    cmd.ExecuteNonQuery();

                    var innerSelectQueryTextAppointment = $"select {nameof(FolderCalendarAppointmentLink.CalendarAppointmentId)} from {nameof(FolderCalendarAppointmentLink)}";
                    var outerDeleteQueryAppointment = $"delete from {nameof(CalendarAppointment)} where {nameof(CalendarAppointment.Id)} not in ({innerSelectQueryTextAppointment}) ";
                    var cmd2 = c.CreateCommand(outerDeleteQueryAppointment);
                    cmd2.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing orphan tasks and appointments.", ex);
            }
        }
    }
}