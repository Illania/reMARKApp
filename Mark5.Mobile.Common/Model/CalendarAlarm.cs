using System;
namespace Mark5.Mobile.Common.Model
{
    public class CalendarAlarm
    {
        public int Id { get; set; } = -1;

        public long AlarmTimestamp { get; set; }

        public int AppointmentId { get; set; } = -1;

        public int CalendarId { get; set; } = -1;
    }
}
