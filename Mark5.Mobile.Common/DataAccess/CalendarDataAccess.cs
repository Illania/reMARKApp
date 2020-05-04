using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.DataAccess
{
    class CalendarDataAccess : ICalendarDataAccess
    {
        readonly DatabaseConnectionProvider calendarDatabase;

        public CalendarDataAccess(DatabaseConnectionProvider calendarDatabase)
        {
            this.calendarDatabase = calendarDatabase;
        }

        public async Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarAppointmentId, int recurrenceIndex = -1)
        {
            try
            {
                CalendarAppointment appointment = null;

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var result = c.Find<CalendarAppointment>(calendarAppointmentId);
                    appointment = result ?? throw new DataNotFoundException("Calendar appointment could not be found");

                    var occurrencesQuery = $"select * from {nameof(CalendarAppointmentOccurrence)} " +
                        $"where {nameof(CalendarAppointmentOccurrence.AppointmentId)} = {calendarAppointmentId} " +
                        $"AND  {nameof(CalendarAppointmentOccurrence.RecurrenceIndex)} = {recurrenceIndex}";

                    var occurrencesResult = c.Query<CalendarAppointmentOccurrence>(occurrencesQuery);
                    if (occurrencesResult == null || occurrencesResult.Count < 1)
                        throw new DataNotFoundException("Calendar appointments occurrences could not be found");

                    appointment.Occurrences.AddRange(occurrencesResult);
                });

                return appointment;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting appointment.", ex);
            }
        }

        public async Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(List<int> calendarIds, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<CalendarAppointment> appointments = null;

                var startDateTimestamp = startDate.ConvertDateTimeToTimestampMilliseconds();
                var endDateTimestamp = endDate.ConvertDateTimeToTimestampMilliseconds();

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var occurrencesQuery = $"select * from {nameof(CalendarAppointmentOccurrence)} " +
                    $"where {nameof(CalendarAppointmentOccurrence.StartDateTimestamp)} <= {endDateTimestamp}" +
                        $" and {nameof(CalendarAppointmentOccurrence.EndDateTimestamp)} >= {startDateTimestamp} and" +
                            $" {nameof(CalendarAppointmentOccurrence.CalendarId)} in ({string.Join(", ", calendarIds)})  ";

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

                    c.Table<CalendarAppointmentOccurrence>().Delete(cao => cao.AppointmentId == calendarAppointment.Id);
                    c.InsertAll(calendarAppointment.Occurrences);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving calendar appointment.", ex);
            }
        }

        public async Task SaveCalendarAppointmentsAsync(List<int> calendarIds, List<CalendarAppointment> calendarAppointments, DateTime startDate, DateTime endDate)
        {
            try
            {
                var startDateTimestamp = startDate.ConvertDateTimeToTimestampMilliseconds();
                var endDateTimestamp = endDate.ConvertDateTimeToTimestampMilliseconds();

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var deleteOccurrencesQuery = $"delete from {nameof(CalendarAppointmentOccurrence)} " +
                                        $"where {nameof(CalendarAppointmentOccurrence.StartDateTimestamp)} <= {endDateTimestamp}" +
                        $" and {nameof(CalendarAppointmentOccurrence.EndDateTimestamp)} >= {startDateTimestamp} and" +
                            $" {nameof(CalendarAppointmentOccurrence.CalendarId)} in ({string.Join(", ", calendarIds)})  ";

                    var deleteResult = c.Execute(deleteOccurrencesQuery);

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
                    c.Table<CalendarAppointment>().Delete(ca => ids.Contains(ca.Id));
                    c.Table<CalendarAppointmentOccurrence>().Delete(cao => ids.Contains(cao.AppointmentId));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting appointments.", ex);
            }
        }

        public async Task RemoveOrphans()
        {
            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var accessibleCalendarIds = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.Select(ca => ca.Id);

                    // 1) Remove all the appointment in calendars for which we do not have access

                    var deleteQuery1 = $"delete from {nameof(CalendarAppointment)}" +
                        $" where {nameof(CalendarAppointment.CalendarId)} not in ({string.Join(", ", accessibleCalendarIds)}) ";

                    var c1 = c.Execute(deleteQuery1);

                    // 2) Remove all occurrences for which we do not have appointments

                    var innerQuery2 = $"select {nameof(CalendarAppointment.Id)} from {nameof(CalendarAppointment)}";

                    var deleteQuery2 = $"delete from {nameof(CalendarAppointmentOccurrence)}" +
                        $" where {nameof(CalendarAppointmentOccurrence.AppointmentId)} not in ({innerQuery2}) ";

                    var c2 = c.Execute(deleteQuery2);

                    // 3) Remove all appointments for which we do not have occurrences

                    var innerQuery3 = $"select 1 from {nameof(CalendarAppointmentOccurrence)} " +
                        $"where {nameof(CalendarAppointmentOccurrence.AppointmentId)} = {nameof(CalendarAppointment.Id)} ";

                    var deleteQuery3 = $"delete from {nameof(CalendarAppointment)}" +
                        $" where not exists ({innerQuery3}) ";

                    var c3 = c.Execute(deleteQuery3);

                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error removing orphan appointments and appointment occcurences.", ex);
            }

        }

        public async Task<List<CalendarReminder>> GetCalendarRemindersAsync()
        {
            try
            {
                List<CalendarReminder> reminders = null;

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    reminders = c.Table<CalendarReminder>().ToList();
                });

                return reminders;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting calendar reminders.", ex);
            }
        }

        public async Task SaveCalendarRemindersAsync(List<CalendarReminder> reminders)
        {
            try
            {
                await calendarDatabase.RunInConnectionAsync(c =>
                        {
                            c.DeleteAll<CalendarReminder>();
                            c.InsertAll(reminders);
                        });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error while savings calendar reminders.", ex);
            }
        }

        public async Task<List<CalendarAlarm>> GetCalendarAlarmsAsync(List<int> calendarIds, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<CalendarAlarm> alarms = null;

                var startDateTimestamp = startDate.ConvertDateTimeToTimestampMilliseconds();
                var endDateTimestamp = endDate.ConvertDateTimeToTimestampMilliseconds();

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var alarmsQuery = $"select * from {nameof(CalendarAlarm)} " +
                    $"where {nameof(CalendarAlarm.AlarmTimestamp)} <= {endDateTimestamp}" +
                        $" and {nameof(CalendarAlarm.AlarmTimestamp)} >= {startDateTimestamp} and" +
                            $" {nameof(CalendarAlarm.CalendarId)} in ({string.Join(", ", calendarIds)})  ";

                    alarms = c.Query<CalendarAlarm>(alarmsQuery);
                    if (alarms == null || alarms.Count < 1)
                        throw new DataNotFoundException("Calendar alarms could not be found");
                });

                return alarms;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting calendar appointments.", ex);
            }
        }

        public async Task SaveCalendarAlarmsAsync(List<int> calendarIds, List<CalendarAlarm> alarms, DateTime startDate, DateTime endDate)
        {
            try
            {
                var startDateTimestamp = startDate.ConvertDateTimeToTimestampMilliseconds();
                var endDateTimestamp = endDate.ConvertDateTimeToTimestampMilliseconds();

                await calendarDatabase.RunInConnectionAsync(c =>
                {
                    var deleteOccurrencesQuery = $"delete from {nameof(CalendarAlarm)} " +
                                        $"where {nameof(CalendarAlarm.AlarmTimestamp)} <= {endDateTimestamp}" +
                        $" and {nameof(CalendarAlarm.AlarmTimestamp)} >= {startDateTimestamp} and" +
                            $" {nameof(CalendarAlarm.CalendarId)} in ({string.Join(", ", calendarIds)})  ";

                    var deleteResult = c.Execute(deleteOccurrencesQuery);

                    c.InsertOrReplaceAll(alarms);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving calendar appointments.", ex);
            }
        }
    }
}