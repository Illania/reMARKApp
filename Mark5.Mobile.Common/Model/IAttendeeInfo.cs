using System;
namespace Mark5.Mobile.Common.Model
{
    public class IAttendeeInfo
    {
        /// <summary> common name </summary>
        public string CN { get; set; } = string.Empty;
        public bool RSVP { get; set; } = true;
        public ParticipantStatus Status { get; set; } = ParticipantStatus.NeedAction;
        public ParticipantType Type { get; set; } = ParticipantType.ComAddress;
        public string Url { get; set; } = string.Empty;
        public bool IsOrganizer { get; set; }
    }
}
