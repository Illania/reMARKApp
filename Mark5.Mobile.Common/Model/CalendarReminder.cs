using System;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    //This class is used for reminders internally by the app
    public class CalendarReminder
    {
        [Column("AppointmentId")]
        public int AppointmentId { get; set; } = -1;

        [Column("CalendarId")]
        public int CalendarId { get; set; } = -1;

        [Column("RecurrenceIndex")]
        public int RecurrenceIndex { get; set; } = -1;

        [Column("AllDay")]
        public bool AllDay { get; set; }

        [Column("Subject")]
        public string Subject { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("Location")]
        public string Location { get; set; }

        [Column("ReminderTime")]
        public DateTime ReminderTime { get; set; }

        [Column("StartTime")]
        public DateTime StartTime { get; set; }
    }
}
