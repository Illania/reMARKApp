using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.Model
{
    public class CalendarInvitation
    {
        public string Id { get; set; }
        public int AppointmentId { get; set; }
        public int CalendarId { get; set; }
        public long StartDateTimestamp { get; set; }
        public long EndDateTimestamp { get; set; }
        public string SerializedTimeZoneInfo { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Summary { get; set; }
        public RecurrenceInfo RecurrenceInfo { get; set; }
        public MethodType MethodType { get; set; }
        public ParticipantStatus Status { get; set; }
        public List<Attendee> Attendees { get; set; }

        public DateTime StartDate => StartDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTimeCalendar();
        public DateTime EndDate => EndDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTimeCalendar();
    }

    public class Attendee
    {
        public string Name { get; set; }
        public ParticipantStatus Status { get; set; }
        public ParticipantType Type { get; set; }
        public bool IsOrganizer { get; set; }
    }
}
