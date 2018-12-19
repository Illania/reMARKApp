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

        public async Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(Folder folder, long startDateTimestamp = -1, long endDateTimestamp = -1)
        {
            try
            {
                List<CalendarAppointment> appointments = null;

                //TODO
                //await calendarDatabase.RunInConnectionAsync(c =>
                //{
                //    var query = $"select * " + $"from {nameof(CalendarAppointment)}, {nameof(FolderCalendarAppointmentLink)} " + $"where {nameof(FolderCalendarAppointmentLink.FolderId)} = {folder.Id} " + $"     and {nameof(CalendarAppointment)}.{nameof(CalendarAppointment.Id)} = {nameof(FolderCalendarAppointmentLink)}.{nameof(FolderCalendarAppointmentLink.CalendarAppointmentId)} ";

                //    if (startDateTimestamp >= 0)
                //        query += $"    and {nameof(CalendarAppointment)}.{nameof(CalendarAppointment.StartDateTimestamp)} >= {startDateTimestamp} ";
                //    if (endDateTimestamp >= 0)
                //        query += $"    and {nameof(CalendarAppointment)}.{nameof(CalendarAppointment.EndDateTimestamp)} <= {endDateTimestamp} ";
                //    query += $"order by {nameof(CalendarAppointment.StartDateTimestamp)} desc ";

                //    var result = c.Query<CalendarAppointment>(query);

                //    if (result == null || result.Count < 1)
                //        throw new DataNotFoundException("Calendar appointments could not be found.");

                //    appointments = result;
                //});

                return appointments;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting calendar appointments.", ex);
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

        public async Task SaveCalendarAppointmentsAsync(Folder folder, IEnumerable<CalendarAppointment> calendarAppointments, bool clean = false)
        {
            try
            {
                //await calendarDatabase.RunInConnectionAsync(c =>
                //{
                //    if (clean)
                //        c.Table<FolderCalendarAppointmentLink>().Delete(fdl => fdl.FolderId == folder.Id);
                //    c.InsertOrReplaceAll(calendarAppointments.Select(ca => new FolderCalendarAppointmentLink
                //    {
                //        FolderId = folder.Id,
                //        CalendarAppointmentId = ca.Id
                //    }));
                //    c.InsertOrReplaceAll(calendarAppointments);
                //});
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving calendar appointments.", ex);
            }
        }


        public async Task DeleteAsync(List<CalendarAppointment> apppointments)
        {
            var ids = apppointments.Select(a => a.Id).Distinct().ToList();

            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    c.Table<CalendarAppointment>().Delete(ca => ids.Contains(ca.Id)); //TODO TO FIX
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting appointments.", ex);
            }
        }

        public async Task RemoveOrphans()
        {
            //TODO maybe we need just to remove all appointments for which we do not have access to the calendar anymore
        }
    }
}