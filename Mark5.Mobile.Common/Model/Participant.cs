namespace Mark5.Mobile.Common.Model
{
    public class Participant
    {
        public int Id { get; set; } = -1;

        public ParticipantPresenence Presence { get; set; }
        public ParticipantType Type { get; set; }
        public ParticipantStatus Status { get; set; }
        public string CN { get; set; }
        public bool Customer { get; set; }
        public string Note { get; set; }  //TODO add emaila
    }
}