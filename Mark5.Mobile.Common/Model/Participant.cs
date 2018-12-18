namespace Mark5.Mobile.Common.Model
{
    public class Participant
    {
        public int Id { get; set; } = -1;
        public ParticipantPresenence Presence { get; set; }
        public ParticipantType Type { get; set; }
        public ParticipantStatus Status { get; set; }
        public string CN { get; set; }
        public string Email { get; set; }
    }
}