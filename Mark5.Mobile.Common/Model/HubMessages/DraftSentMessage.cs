using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class DraftSentMessage : TinyMessageBase
    {
        public int DocumentId { get; }

        public DraftSentMessage(object sender, int documentId)
            : base(sender)
        {
            DocumentId = documentId;
        }
    }
}