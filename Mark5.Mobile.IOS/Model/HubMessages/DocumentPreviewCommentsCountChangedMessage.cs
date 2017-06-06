using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class DocumentPreviewCommentsCountChangedMessage : TinyMessageBase
    {
        public int DocumentPreviewId { get; }

        public int CommentsCount { get; }

        public DocumentPreviewCommentsCountChangedMessage(object sender, int documentId, int commentCount)
            : base(sender)
        {
            DocumentPreviewId = documentId;
            CommentsCount = commentCount;
        }
    }
}