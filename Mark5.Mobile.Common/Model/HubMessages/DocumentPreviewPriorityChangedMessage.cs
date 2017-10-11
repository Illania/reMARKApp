using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class DocumentPreviewPriorityChangedMessage : TinyMessageBase
    {
        public int DocumentPreviewId { get; }

        public Priority Priority { get; }

        public DocumentPreviewPriorityChangedMessage(object sender, int documentPreviewId, Priority priority)
            : base(sender)
        {
            DocumentPreviewId = documentPreviewId;
            Priority = priority;
        }
    }
}