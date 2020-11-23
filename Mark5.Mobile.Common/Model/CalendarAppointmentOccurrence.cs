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

        [Column("ChangedOccurenceId")]
        public int ChangedOccurenceId { get; set; }

        [Column("Subject")]
        public string Subject { get; set; }

        public DateTime StartDate
        {
            get => StartDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTimeCalendar();
            set { StartDateTimestamp = value.ConvertUserTimeToUtcCalendar().ConvertDateTimeToTimestampMilliseconds(); }
        }

        public DateTime EndDate
        {
            get => EndDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTimeCalendar();
            set { EndDateTimestamp = value.ConvertUserTimeToUtcCalendar().ConvertDateTimeToTimestampMilliseconds(); }
        }

        public DateTime AllDayStartDate => StartDateTimestamp.ConvertTimestampMillisecondsToDateTime();

        public DateTime AllDayEndDate => EndDateTimestamp.ConvertTimestampMillisecondsToDateTime();

        public override string ToString()
        {
            return string.Format("[CalendarAppointmentOccurrence: RecurrenceIndex={0}, StartDate={1}, EndDate={2}]", RecurrenceIndex, StartDate, EndDate);
        }

    }
}
