using System.Linq;

namespace Mark5.Mobile.Common.Model
{
    public class IEventReply
    {
        public string EventId { get; set; }

        public ParticipantStatus ParticipantStatus { get; set; }

        public bool Silent { get; set; }

        public IEventReply(Document originalDocument, ParticipantStatus participantStatus, bool silent)
        {
            if (originalDocument != null)
            {
                if (originalDocument.ICalendars.Any() && originalDocument.ICalendars.First().Events.Any())
                {
                    EventId = originalDocument.ICalendars.First().Events.First().Id;
                }
            }

            Silent = silent;

            ParticipantStatus = participantStatus;
        }
    }
}
