using SQLite;

namespace Mark5.Mobile.Common.Model
{
    [Table("DocumentPreview")]
    public class CalendarAppointmentOccurrence
    {
        [Column("StartDateTimestamp")]
        public long StartDateTimestamp { get; set; }

        [Column("EndDateTimestamp")]
        public long EndDateTimestamp { get; set; }

        [Column("ReferenceNumber")]
        public int RecurrenceIndex { get; set; }

        [Column("AppointmentId")]
        public int AppointmentId { get; set; }
    }
}
