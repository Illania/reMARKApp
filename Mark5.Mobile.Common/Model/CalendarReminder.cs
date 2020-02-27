using System;
namespace Mark5.Mobile.Common.Model
{
    //This class is used for reminders internally by the app
    public class CalendarReminder
    {
        public int AppointmentId { get; set; } = -1;

        public int CalendarId { get; set; } = -1;

        public int RecurrenceIndex { get; set; } = -1;

        public bool AllDay { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public DateTime ReminderTime { get; set; }

        public DateTime StartTime { get; set; }

    }
}
