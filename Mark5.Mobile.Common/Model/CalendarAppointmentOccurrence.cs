using System;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    [Table("CalendarAppointmentOccurrence")]
    public class CalendarAppointmentOccurrence
    {
        [Column("StartDateTimestamp")]
        public long StartDateTimestamp { get; set; }

        [Column("EndDateTimestamp")]
        public long EndDateTimestamp { get; set; }

        [Column("RecurrenceIndex")]
        public int RecurrenceIndex { get; set; }

        [Column("AppointmentId")]
        public int AppointmentId { get; set; }

        [Column("CalendarId")]
        public int CalendarId { get; set; }

        public DateTime StartDate => StartDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
        public DateTime EndDate => EndDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();

    }
}
