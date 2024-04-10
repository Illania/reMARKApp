using TinyMessenger;

namespace reMark.Mobile.Common.Model.HubMessages
{
    public class DocumentPreviewPriorityChangedMessage : TinyMessageBase, IMessageWithId
    {
        public int DocumentPreviewId { get; }

        public Priority Priority { get; }
        
        public int Id => DocumentPreviewId;

        public DocumentPreviewPriorityChangedMessage(object sender, int documentPreviewId, Priority priority)
            : base(sender)
        {
            DocumentPreviewId = documentPreviewId;
            Priority = priority;
        }
    }
}
