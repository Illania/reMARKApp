using TinyMessenger;

namespace reMark.Mobile.IOS.Model.HubMessages
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
