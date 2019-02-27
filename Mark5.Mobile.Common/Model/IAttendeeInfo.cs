using System;
namespace Mark5.Mobile.Common.Model
{
    public class IAttendeeInfo
    {
        /// <summary> common name </summary>
        public string CN { get; set; }
        public bool RSVP { get; set; }
        public ParticipantStatus Status { get; set; }
        public ParticipantType Type { get; set; }
        public string Url { get; set; }
        public bool IsOrganizer { get; set; }
    }
}
