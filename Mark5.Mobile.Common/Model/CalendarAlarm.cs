using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class CalendarAlarm
    {
        [Column("Id")]
        public int Id { get; set; } = -1;

        [Column("AlarmTimestamp")]
        public long AlarmTimestamp { get; set; }

        [Column("AppointmentId")]
        public int AppointmentId { get; set; } = -1;

        [Column("CalendarId")]
        public int CalendarId { get; set; } = -1;
    }
}
