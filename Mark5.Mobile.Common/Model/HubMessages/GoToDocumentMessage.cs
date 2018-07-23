using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class GoToDocumentMessage : TinyMessageBase
    {
        public int DocumentId { get; }

        public GoToDocumentMessage(object sender, int documentId)
            : base(sender)
        {
            DocumentId = documentId;
        }
    }
}