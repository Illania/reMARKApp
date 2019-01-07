using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;

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
                    appointment = result ?? throw new DataNotFoundException("Calendar appointment could not be found");

                    var occurrencesQuery = $"select * from {nameof(CalendarAppointmentOccurrence)} " +
                        $"where {nameof(CalendarAppointmentOccurrence.AppointmentId)} = {calendarAppointmentId} ";

                    var occurrencesResult = c.Query<CalendarAppointmentOccurrence>(occurrencesQuery);
                    if (occurrencesResult == null || occurrencesResult.Count < 1)
                        throw new DataNotFoundException("Calendar appointments occurrences could not be found");

                });

                return appointment;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting appointment.", ex);
            }
        }

        public async Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(IEnumerable<int> calendarIds, long startDateTimestamp = -1, long endDateTimestamp = -1)
        {
            try
            {
                List<CalendarAppointment> appointments = null;

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var occurrencesQuery = $"select {nameof(CalendarAppointmentOccurrence.AppointmentId)}, {nameof(CalendarAppointmentOccurrence.RecurrenceIndex)}," +
                        $" {nameof(CalendarAppointmentOccurrence.StartDateTimestamp)},  {nameof(CalendarAppointmentOccurrence.EndDateTimestamp)}  " +
                        $"from {nameof(CalendarAppointmentOccurrence)}, {nameof(CalendarAppointment)} " +
                    $"where {nameof(CalendarAppointmentOccurrence.StartDateTimestamp)} <= {endDateTimestamp}" +
                        $" and {nameof(CalendarAppointmentOccurrence.EndDateTimestamp)} >= {startDateTimestamp} and" +
                            $" {nameof(CalendarAppointment.CalendarId)} in ({string.Join(", ", calendarIds)})  ";

                    var occurrencesResult = c.Query<CalendarAppointmentOccurrence>(occurrencesQuery);
                    if (occurrencesResult == null || occurrencesResult.Count < 1)
                        throw new DataNotFoundException("Calendar appointments occurrences could not be found");

                    var apIds = occurrencesResult.Select(co => co.AppointmentId);
                    appointments = c.Table<CalendarAppointment>().Where(ca => apIds.Contains(ca.Id)).ToList();
                    if (appointments == null || !appointments.Any())
                        throw new DataNotFoundException("Calendar appointments could not be found");

                    foreach (var ap in appointments)
                        ap.Occurrences.AddRange(occurrencesResult.Where(oc => oc.AppointmentId == ap.Id));

                });

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
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(calendarAppointment);
                    c.InsertOrReplace(calendarAppointment.Occurrences);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving calendar appointment.", ex);
            }
        }

        public async Task SaveCalendarAppointmentsAsync(IEnumerable<CalendarAppointment> calendarAppointments, long startDateTimestamp, long endDateTimestamp)
        {
            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplaceAll(calendarAppointments);
                    c.InsertOrReplaceAll(calendarAppointments.Select(ca => ca.Occurrences).SelectMany(i => i));
                });
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