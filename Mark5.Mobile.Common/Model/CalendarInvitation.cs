using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class CalendarInvitation
    {
        public string Id { get; set; }

        public int AppointmentId { get; set; }

        public int CalendarId { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string SerializedTimeZoneInfo { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public string Summary { get; set; }

        public RecurrenceInfo RecurrenceInfo { get; set; }

        public MethodType MethodType { get; set; }

        public ParticipantStatus Status { get; set; }

        public List<Attendee> Attendees { get; set; }
    }

    public class Attendee
    {
        public string Name { get; set; }

        public ParticipantStatus Status { get; set; }

        public ParticipantType Type { get; set; }

        public bool IsOrganizer { get; set; }
    }
}
